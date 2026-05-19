using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class InformesService(AppDbContext db)
{
    public async Task<InformesViewModel> ObtenerInformesAsync(DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        DateTime? d = desde?.Date;
        DateTime? h = hasta?.Date.AddDays(1).AddTicks(-1);

        var vm = new InformesViewModel { Desde = desde?.Date, Hasta = hasta?.Date };

        List<Venta> ventas = await db.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.IdVenta)
            .ToListAsync(ct);

        ventas = ventas.Where(v => EnRango(TextoToFecha(v.Fecha), d, h)).ToList();

        foreach (Venta v in ventas)
        {
            DateTime fecha = TextoToFecha(v.Fecha);
            int unidades = v.Detalles.Sum(x => x.Cantidad);
            string detalle = string.Join("; ", v.Detalles.Select(d =>
                $"{d.Producto?.Nombre ?? "Producto"} x{d.Cantidad}"));

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

        var movimientos = await (
            from s in db.MovimientosStock.AsNoTracking()
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join p in db.Productos on s.IdProducto equals p.IdProducto
            join c in db.Categorias on p.IdCategoria equals c.IdCategoria
            orderby i.IdInventario descending
            select new
            {
                i.IdInventario,
                i.Fecha,
                p.Nombre,
                p.CodigoBarras,
                c.NombreCategoria,
                s.Cantidad,
                s.StockMinimo
            }).ToListAsync(ct);

        foreach (var m in movimientos)
        {
            DateTime fecha = DateTime.TryParse(m.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fd)
                ? fd
                : DateTime.MinValue;
            if (!EnRango(fecha, d, h))
            {
                continue;
            }

            if (m.Cantidad > 0)
            {
                vm.EntradasInventario.Add(new InformeEntradaInventarioFila
                {
                    IdInventario = m.IdInventario,
                    Fecha = fecha,
                    NombreProducto = m.Nombre,
                    CodigoBarras = m.CodigoBarras,
                    NombreCategoria = m.NombreCategoria,
                    Cantidad = m.Cantidad,
                    StockMinimo = m.StockMinimo
                });
            }
            else if (m.Cantidad < 0)
            {
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

        var detallesFiltrados = ventas.SelectMany(v => v.Detalles).ToList();
        vm.ProductosMasVendidos = detallesFiltrados
            .GroupBy(d => d.IdProducto)
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

        return vm;
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
