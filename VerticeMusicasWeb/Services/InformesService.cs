using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class InformesService(AppDbContext db)
{
    public async Task<InformesViewModel> ObtenerInformesAsync(
        DateTime? desde,
        DateTime? hasta,
        int? idProductoComparar = null,
        int? idProveedor = null,
        int? idCategoria = null,
        string? q = null,
        CancellationToken ct = default)
    {
        DateTime? d = desde?.Date;
        DateTime? h = hasta?.Date.AddDays(1).AddTicks(-1);

        var vm = new InformesViewModel
        {
            Desde = desde?.Date,
            Hasta = hasta?.Date,
            IdProductoComparar = idProductoComparar,
            IdProveedor = idProveedor,
            IdCategoria = idCategoria,
            TerminoBusqueda = q
        };

        vm.Resumen.TotalProductos = await db.Productos.CountAsync(ct);
        vm.Resumen.TotalCategorias = await db.Categorias.CountAsync(ct);
        vm.Resumen.TotalProveedores = await db.Proveedores.CountAsync(ct);

        await CargarVentasAsync(vm, d, h, idCategoria, q, ct);
        await CargarMovimientosInventarioAsync(vm, d, h, idProveedor, idCategoria, q, ct);
        await CargarProductosCatalogoAsync(vm, idProveedor, idCategoria, q, ct);
        await CargarCategoriasAsync(vm, ct);
        await CargarProveedoresAsync(vm, d, h, idProveedor, ct);
        await CargarComparacionPreciosAsync(vm, d, h, idProductoComparar, idProveedor, idCategoria, q, ct);

        return vm;
    }

    private async Task CargarVentasAsync(
        InformesViewModel vm,
        DateTime? d,
        DateTime? h,
        int? idCategoria,
        string? q,
        CancellationToken ct)
    {
        List<Venta> ventas = await db.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
                .ThenInclude(det => det.Producto)
            .OrderByDescending(v => v.IdVenta)
            .ToListAsync(ct);

        ventas = ventas.Where(v => EnRango(TextoToFecha(v.Fecha), d, h)).ToList();

        foreach (Venta v in ventas)
        {
            DateTime fecha = TextoToFecha(v.Fecha);
            int unidades = v.Detalles.Sum(x => x.Cantidad);
            string detalle = string.Join("; ", v.Detalles.Select(det =>
                $"{det.Producto?.Nombre ?? "Producto"} x{det.Cantidad}"));

            vm.Ventas.Add(new InformeVentaFila
            {
                IdVenta = v.IdVenta,
                Fecha = fecha,
                MetodoPago = v.MetodoPago,
                Total = v.Total,
                CantidadLineas = v.Detalles.Count,
                Unidades = unidades,
                DetalleProductos = detalle
            });
        }

        vm.Resumen.TotalVentas = vm.Ventas.Count;
        vm.Resumen.MontoTotalVentas = vm.Ventas.Sum(x => x.Total);
        vm.Resumen.UnidadesVendidas = vm.Ventas.Sum(x => x.Unidades);

        var detallesFiltrados = ventas.SelectMany(v => v.Detalles).AsEnumerable();
        if (idCategoria.HasValue)
        {
            int cid = idCategoria.Value;
            detallesFiltrados = detallesFiltrados.Where(det => det.Producto?.IdCategoria == cid);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            string term = q.Trim();
            detallesFiltrados = detallesFiltrados.Where(det =>
                (det.Producto?.Nombre?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (det.Producto?.CodigoBarras?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        vm.ProductosMasVendidos = detallesFiltrados
            .GroupBy(det => det.IdProducto)
            .Select(g =>
            {
                string nombre = g.First().Producto?.Nombre ?? "Producto";
                string codigo = g.First().Producto?.CodigoBarras ?? "-";
                return new InformeProductoVendidoFila
                {
                    NombreProducto = nombre,
                    CodigoBarras = codigo,
                    UnidadesVendidas = g.Sum(x => x.Cantidad),
                    MontoTotal = g.Sum(x => x.Cantidad * x.PrecioUnitario)
                };
            })
            .OrderByDescending(x => x.UnidadesVendidas)
            .Take(20)
            .ToList();
    }

    private async Task CargarMovimientosInventarioAsync(
        InformesViewModel vm,
        DateTime? d,
        DateTime? h,
        int? idProveedor,
        int? idCategoria,
        string? q,
        CancellationToken ct)
    {
        var movimientos = await (
            from s in db.MovimientosStock.AsNoTracking()
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join p in db.Productos on s.IdProducto equals p.IdProducto
            join c in db.Categorias on p.IdCategoria equals c.IdCategoria
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor into proveedores
            from pr in proveedores.DefaultIfEmpty()
            orderby i.IdInventario descending
            select new
            {
                i.IdInventario,
                i.Fecha,
                p.IdProducto,
                p.IdCategoria,
                p.Nombre,
                p.CodigoBarras,
                c.NombreCategoria,
                s.IdProveedor,
                NombreProveedor = pr != null ? pr.Nombre : null,
                s.Cantidad,
                s.StockMinimo,
                s.PrecioUnitarioCompra
            }).ToListAsync(ct);

        decimal montoCompras = 0;
        string? termino = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        foreach (var m in movimientos)
        {
            DateTime fecha = DateTime.TryParse(m.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fd)
                ? fd
                : DateTime.MinValue;
            if (!EnRango(fecha, d, h))
            {
                continue;
            }

            if (idCategoria.HasValue && m.IdCategoria != idCategoria.Value)
            {
                continue;
            }

            if (termino is not null &&
                !m.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) &&
                !m.CodigoBarras.Contains(termino, StringComparison.OrdinalIgnoreCase) &&
                !(m.NombreProveedor?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            if (idProveedor.HasValue && m.Cantidad > 0 && m.IdProveedor != idProveedor.Value)
            {
                continue;
            }

            if (m.Cantidad > 0)
            {
                decimal totalLinea = (m.PrecioUnitarioCompra ?? 0) * m.Cantidad;
                montoCompras += totalLinea;

                vm.EntradasInventario.Add(new InformeEntradaInventarioFila
                {
                    IdInventario = m.IdInventario,
                    Fecha = fecha,
                    NombreProducto = m.Nombre,
                    CodigoBarras = m.CodigoBarras,
                    NombreCategoria = m.NombreCategoria,
                    NombreProveedor = m.NombreProveedor ?? "Sin proveedor",
                    Cantidad = m.Cantidad,
                    StockMinimo = m.StockMinimo,
                    PrecioUnitarioCompra = m.PrecioUnitarioCompra
                });

                if (m.PrecioUnitarioCompra.HasValue && m.PrecioUnitarioCompra > 0)
                {
                    vm.ComprasPorProveedor.Add(new InformeCompraProveedorFila
                    {
                        Fecha = fecha,
                        NombreProveedor = m.NombreProveedor ?? "Sin proveedor",
                        NombreProducto = m.Nombre,
                        CodigoBarras = m.CodigoBarras,
                        Cantidad = m.Cantidad,
                        PrecioUnitario = m.PrecioUnitarioCompra.Value,
                        TotalLinea = totalLinea
                    });
                }
            }
            else if (m.Cantidad < 0)
            {
                if (idProveedor.HasValue)
                {
                    continue;
                }

                vm.SalidasInventario.Add(new InformeSalidaInventarioFila
                {
                    IdInventario = m.IdInventario,
                    Fecha = fecha,
                    NombreProducto = m.Nombre,
                    CodigoBarras = m.CodigoBarras,
                    NombreCategoria = m.NombreCategoria,
                    Cantidad = Math.Abs(m.Cantidad),
                    Motivo = "Salida por venta u otro movimiento"
                });
            }
        }

        vm.Resumen.EntradasInventario = vm.EntradasInventario.Count;
        vm.Resumen.UnidadesIngresadas = vm.EntradasInventario.Sum(x => x.Cantidad);
        vm.Resumen.SalidasInventario = vm.SalidasInventario.Count;
        vm.Resumen.UnidadesRetiradas = vm.SalidasInventario.Sum(x => x.Cantidad);
        vm.Resumen.MontoTotalCompras = montoCompras;
        vm.ComprasPorProveedor = vm.ComprasPorProveedor
            .OrderByDescending(x => x.Fecha)
            .ToList();
    }

    private async Task CargarProductosCatalogoAsync(
        InformesViewModel vm,
        int? idProveedor,
        int? idCategoria,
        string? q,
        CancellationToken ct)
    {
        IQueryable<Producto> query = db.Productos.AsNoTracking().Include(p => p.Categoria);

        if (idCategoria.HasValue)
        {
            int cid = idCategoria.Value;
            query = query.Where(p => p.IdCategoria == cid);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            string like = $"%{q.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, like) ||
                EF.Functions.Like(p.CodigoBarras, like));
        }

        if (idProveedor.HasValue)
        {
            int pid = idProveedor.Value;
            query = query.Where(p => p.MovimientosStock.Any(m =>
                m.Cantidad > 0 && m.IdProveedor == pid));
        }

        var productos = await query
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.CodigoBarras,
                NombreCategoria = p.Categoria!.NombreCategoria,
                p.Marca,
                p.Precio,
                Stock = p.MovimientosStock.Sum(m => (int?)m.Cantidad) ?? 0
            })
            .ToListAsync(ct);

        var comprasRaw = await (
            from s in db.MovimientosStock.AsNoTracking()
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor
            where s.Cantidad > 0 && s.PrecioUnitarioCompra != null
            select new { s.IdProducto, s.PrecioUnitarioCompra, pr.Nombre, s.IdProveedor }
        ).ToListAsync(ct);

        var preciosPorProducto = comprasRaw
            .GroupBy(x => x.IdProducto)
            .Select(g =>
            {
                var mejor = g.OrderBy(x => x.PrecioUnitarioCompra).First();
                return new
                {
                    IdProducto = g.Key,
                    MejorPrecio = mejor.PrecioUnitarioCompra,
                    Proveedor = mejor.Nombre
                };
            })
            .ToList();

        foreach (var p in productos)
        {
            var mejor = preciosPorProducto.FirstOrDefault(x => x.IdProducto == p.IdProducto);
            vm.ProductosCatalogo.Add(new InformeProductoCatalogoFila
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                NombreCategoria = p.NombreCategoria,
                Marca = p.Marca,
                PrecioVenta = p.Precio,
                StockDisponible = p.Stock,
                MejorProveedor = mejor?.Proveedor,
                MejorPrecioCompra = mejor?.MejorPrecio
            });
        }
    }

    private async Task CargarCategoriasAsync(InformesViewModel vm, CancellationToken ct)
    {
        vm.Categorias = await db.Categorias.AsNoTracking()
            .OrderBy(c => c.NombreCategoria)
            .Select(c => new InformeCategoriaFila
            {
                IdCategoria = c.IdCategoria,
                NombreCategoria = c.NombreCategoria,
                CantidadProductos = c.Productos.Count,
                StockTotal = c.Productos.SelectMany(p => p.MovimientosStock).Sum(m => (int?)m.Cantidad) ?? 0
            })
            .ToListAsync(ct);
    }

    private async Task CargarProveedoresAsync(
        InformesViewModel vm,
        DateTime? d,
        DateTime? h,
        int? idProveedor,
        CancellationToken ct)
    {
        IQueryable<Proveedor> queryProveedores = db.Proveedores.AsNoTracking();
        if (idProveedor.HasValue)
        {
            int pid = idProveedor.Value;
            queryProveedores = queryProveedores.Where(p => p.IdProveedor == pid);
        }

        List<Proveedor> proveedores = await queryProveedores.OrderBy(p => p.Nombre).ToListAsync(ct);

        var compras = await (
            from s in db.MovimientosStock.AsNoTracking()
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor
            where s.Cantidad > 0
            select new
            {
                pr.IdProveedor,
                pr.Nombre,
                pr.Contacto,
                i.Fecha,
                s.IdProducto,
                s.Cantidad,
                s.PrecioUnitarioCompra
            }).ToListAsync(ct);

        foreach (Proveedor pr in proveedores)
        {
            var lineas = compras
                .Where(x => x.IdProveedor == pr.IdProveedor)
                .Where(x =>
                {
                    DateTime fecha = DateTime.TryParse(x.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fd)
                        ? fd
                        : DateTime.MinValue;
                    return EnRango(fecha, d, h);
                })
                .ToList();

            vm.Proveedores.Add(new InformeProveedorResumenFila
            {
                IdProveedor = pr.IdProveedor,
                NombreProveedor = pr.Nombre,
                Contacto = pr.Contacto,
                Nit = pr.Nit,
                PersonaContacto = pr.PersonaContacto,
                Celular = pr.Celular,
                CorreoElectronico = pr.CorreoElectronico,
                TelefonoFijo = pr.TelefonoFijo,
                Ciudad = pr.Ciudad,
                Direccion = pr.Direccion,
                NumeroCompras = lineas.Count,
                UnidadesCompradas = lineas.Sum(x => x.Cantidad),
                MontoTotalCompras = lineas.Sum(x => (x.PrecioUnitarioCompra ?? 0) * x.Cantidad),
                ProductosDistintos = lineas.Select(x => x.IdProducto).Distinct().Count()
            });
        }

        vm.Proveedores = vm.Proveedores
            .OrderByDescending(x => x.MontoTotalCompras)
            .ToList();
    }

    private async Task CargarComparacionPreciosAsync(
        InformesViewModel vm,
        DateTime? d,
        DateTime? h,
        int? idProductoComparar,
        int? idProveedor,
        int? idCategoria,
        string? q,
        CancellationToken ct)
    {
        var compras = await (
            from s in db.MovimientosStock.AsNoTracking()
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join p in db.Productos on s.IdProducto equals p.IdProducto
            join c in db.Categorias on p.IdCategoria equals c.IdCategoria
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor
            where s.Cantidad > 0 && s.PrecioUnitarioCompra != null
            select new CompraPrecioRow
            {
                IdProducto = p.IdProducto,
                IdCategoria = p.IdCategoria,
                NombreProducto = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                NombreCategoria = c.NombreCategoria,
                PrecioVenta = p.Precio,
                IdProveedor = pr.IdProveedor,
                NombreProveedor = pr.Nombre,
                PrecioUnitarioCompra = s.PrecioUnitarioCompra!.Value,
                FechaTexto = i.Fecha,
                IdStock = s.IdStock
            }).ToListAsync(ct);

        string? termino = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        IEnumerable<IGrouping<int, CompraPrecioRow>> grupos = compras
            .Where(x => EnRango(TextoToFecha(x.FechaTexto), d, h))
            .Where(x => !idCategoria.HasValue || x.IdCategoria == idCategoria.Value)
            .Where(x => !idProveedor.HasValue || x.IdProveedor == idProveedor.Value)
            .Where(x => termino is null ||
                x.NombreProducto.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                x.CodigoBarras.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                x.NombreProveedor.Contains(termino, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.IdProducto);

        if (idProductoComparar.HasValue)
        {
            grupos = grupos.Where(g => g.Key == idProductoComparar.Value);
        }

        foreach (IGrouping<int, CompraPrecioRow> grupo in grupos)
        {
            CompraPrecioRow first = grupo.First();
            List<InformePrecioProveedorItem> items = grupo
                .GroupBy(x => x.IdProveedor)
                .Select(g =>
                {
                    CompraPrecioRow ultima = g.OrderByDescending(x => x.IdStock).First();
                    return new InformePrecioProveedorItem
                    {
                        IdProveedor = g.Key,
                        NombreProveedor = ultima.NombreProveedor,
                        PrecioMinimo = g.Min(x => x.PrecioUnitarioCompra),
                        PrecioMaximo = g.Max(x => x.PrecioUnitarioCompra),
                        PrecioPromedio = Math.Round(g.Average(x => x.PrecioUnitarioCompra), 2),
                        UltimoPrecio = ultima.PrecioUnitarioCompra,
                        FechaUltimaCompra = TextoToFecha(ultima.FechaTexto),
                        NumeroCompras = g.Count()
                    };
                })
                .OrderBy(x => x.UltimoPrecio)
                .ToList();

            if (items.Count < 2 && !idProductoComparar.HasValue)
            {
                continue;
            }

            vm.ComparacionPreciosProveedores.Add(new InformeComparacionPrecioFila
            {
                IdProducto = grupo.Key,
                NombreProducto = first.NombreProducto,
                CodigoBarras = first.CodigoBarras,
                NombreCategoria = first.NombreCategoria,
                PrecioVenta = first.PrecioVenta,
                PreciosPorProveedor = items
            });
        }

        vm.ComparacionPreciosProveedores = vm.ComparacionPreciosProveedores
            .OrderBy(x => x.NombreProducto)
            .ToList();
    }

    private sealed class CompraPrecioRow
    {
        public int IdProducto { get; init; }
        public int IdCategoria { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string CodigoBarras { get; init; } = string.Empty;
        public string NombreCategoria { get; init; } = string.Empty;
        public decimal PrecioVenta { get; init; }
        public int IdProveedor { get; init; }
        public string NombreProveedor { get; init; } = string.Empty;
        public decimal PrecioUnitarioCompra { get; init; }
        public string FechaTexto { get; init; } = string.Empty;
        public int IdStock { get; init; }
    }

    private static bool EnRango(DateTime fecha, DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue && fecha < desde.Value)
        {
            return false;
        }

        if (hasta.HasValue && fecha > hasta.Value)
        {
            return false;
        }

        return true;
    }

    private static DateTime TextoToFecha(string fechaTexto)
    {
        if (DateTime.TryParseExact(
            fechaTexto,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime fecha))
        {
            return fecha;
        }

        return DateTime.TryParse(fechaTexto, out DateTime f) ? f : DateTime.MinValue;
    }
}
