using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class InventarioStockService(AppDbContext db)
{
    private static string FechaToTexto(DateTime fecha) =>
        fecha.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public async Task<int> RegistrarMovimientoAsync(DateTime fecha, int idProducto, int cantidad, CancellationToken ct = default)
    {
        return await RegistrarMovimientoAsync(fecha, idProducto, cantidad, 0, null, null, ct);
    }

    public async Task<int> RegistrarMovimientoAsync(
        DateTime fecha,
        int idProducto,
        int cantidad,
        int stockMinimo,
        CancellationToken ct) =>
        await RegistrarMovimientoAsync(fecha, idProducto, cantidad, stockMinimo, null, null, ct);

    public async Task<int> RegistrarMovimientoAsync(
        DateTime fecha,
        int idProducto,
        int cantidad,
        int stockMinimo,
        int? idProveedor = null,
        decimal? precioUnitarioCompra = null,
        CancellationToken ct = default)
    {
        if (cantidad == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad no puede ser cero.");
        }

        if (stockMinimo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockMinimo), "El stock minimo no puede ser negativo.");
        }

        if (cantidad > 0)
        {
            if (!idProveedor.HasValue || idProveedor.Value <= 0)
            {
                throw new InvalidOperationException("Debe seleccionar un proveedor para la entrada de inventario.");
            }

            if (!precioUnitarioCompra.HasValue || precioUnitarioCompra.Value <= 0)
            {
                throw new InvalidOperationException("El precio de compra debe ser mayor a cero.");
            }

            bool existeProveedor = await db.Proveedores.AnyAsync(p => p.IdProveedor == idProveedor.Value, ct);
            if (!existeProveedor)
            {
                throw new InvalidOperationException("El proveedor seleccionado no existe.");
            }
        }

        bool existeProducto = await db.Productos.AnyAsync(p => p.IdProducto == idProducto, ct);
        if (!existeProducto)
        {
            throw new InvalidOperationException("El producto no existe.");
        }

        bool usaTransaccionExistente = db.Database.CurrentTransaction is not null;
        await using var tx = usaTransaccionExistente ? null : await db.Database.BeginTransactionAsync(ct);
        try
        {
            var inv = new Inventario { Fecha = FechaToTexto(fecha) };
            db.Inventarios.Add(inv);
            await db.SaveChangesAsync(ct);

            db.MovimientosStock.Add(new MovimientoStock
            {
                Cantidad = cantidad,
                StockMinimo = stockMinimo,
                IdInventario = inv.IdInventario,
                IdProducto = idProducto,
                IdProveedor = cantidad > 0 ? idProveedor : null,
                PrecioUnitarioCompra = cantidad > 0 ? precioUnitarioCompra : null
            });
            await db.SaveChangesAsync(ct);
            if (!usaTransaccionExistente && tx is not null)
            {
                await tx.CommitAsync(ct);
            }
            return inv.IdInventario;
        }
        catch
        {
            if (!usaTransaccionExistente && tx is not null)
            {
                await tx.RollbackAsync(ct);
            }
            throw;
        }
    }

    public async Task<int> ObtenerStockCantidadActualAsync(int idProducto, CancellationToken ct = default)
    {
        int sum = await db.MovimientosStock
            .Where(m => m.IdProducto == idProducto)
            .SumAsync(m => (int?)m.Cantidad, ct) ?? 0;
        return sum;
    }

    public async Task<List<InventarioMovimientoRow>> GetMovimientosAsync(
        int? idCategoria,
        string? term,
        int? idProveedor = null,
        CancellationToken ct = default)
    {
        bool tieneTerm = !string.IsNullOrWhiteSpace(term);
        string? normalized = tieneTerm ? term!.Trim() : null;

        var query =
            from s in db.MovimientosStock
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join p in db.Productos on s.IdProducto equals p.IdProducto
            join c in db.Categorias on p.IdCategoria equals c.IdCategoria
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor into proveedores
            from pr in proveedores.DefaultIfEmpty()
            select new { s, i, p, c, pr };

        if (idCategoria.HasValue)
        {
            int cid = idCategoria.Value;
            query = query.Where(x => x.c.IdCategoria == cid);
        }

        if (idProveedor.HasValue)
        {
            int pid = idProveedor.Value;
            query = query.Where(x => x.s.IdProveedor == pid);
        }

        if (tieneTerm && normalized is not null)
        {
            string like = $"%{normalized}%";
            query = query.Where(x =>
                EF.Functions.Like(x.p.Nombre, like) ||
                x.p.CodigoBarras == normalized ||
                EF.Functions.Like(x.p.CodigoBarras, like));
        }

        var raw = await query
            .OrderByDescending(x => x.i.IdInventario)
            .Select(x => new
            {
                x.i.IdInventario,
                x.i.Fecha,
                x.p.IdProducto,
                NombreProducto = x.p.Nombre,
                x.p.CodigoBarras,
                NombreCategoria = x.c.NombreCategoria,
                x.s.Cantidad,
                NombreProveedor = x.pr != null ? x.pr.Nombre : null,
                x.s.PrecioUnitarioCompra
            })
            .ToListAsync(ct);

        return raw.ConvertAll(x => new InventarioMovimientoRow
        {
            IdInventario = x.IdInventario,
            Fecha = DateTime.TryParse(x.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fd)
                ? fd
                : DateTime.MinValue,
            IdProducto = x.IdProducto,
            NombreProducto = x.NombreProducto,
            CodigoBarras = x.CodigoBarras,
            NombreCategoria = x.NombreCategoria,
            Cantidad = x.Cantidad,
            NombreProveedor = x.NombreProveedor,
            PrecioUnitarioCompra = x.PrecioUnitarioCompra
        });
    }

    public async Task<List<StockDisponibleRow>> GetStockDisponibleAsync(
        int? idCategoria,
        string? term,
        int? idProveedor = null,
        CancellationToken ct = default)
    {
        bool tieneTerm = !string.IsNullOrWhiteSpace(term);
        string? normalized = tieneTerm ? term!.Trim() : null;

        IQueryable<Producto> productos = db.Productos.AsNoTracking();

        if (idCategoria.HasValue)
        {
            int cid = idCategoria.Value;
            productos = productos.Where(p => p.IdCategoria == cid);
        }

        if (tieneTerm && normalized is not null)
        {
            string like = $"%{normalized}%";
            productos = productos.Where(p =>
                EF.Functions.Like(p.Nombre, like) ||
                p.CodigoBarras == normalized ||
                EF.Functions.Like(p.CodigoBarras, like));
        }

        if (idProveedor.HasValue)
        {
            int pid = idProveedor.Value;
            productos = productos.Where(p => p.MovimientosStock.Any(m =>
                m.Cantidad > 0 && m.IdProveedor == pid));
        }

        var lista = await productos
            .Select(p => new StockDisponibleRow
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                NombreCategoria = p.Categoria!.NombreCategoria,
                CantidadDisponible = p.MovimientosStock.Sum(m => (int?)m.Cantidad) ?? 0,
                StockMinimo = p.MovimientosStock.Select(m => (int?)m.StockMinimo).Max() ?? 0,
                ManejaStock = p.ManejaStock
            })
            .OrderBy(x => x.CantidadDisponible)
            .ToListAsync(ct);

        if (lista.Count == 0)
        {
            return lista;
        }

        var ids = lista.Select(x => x.IdProducto).ToList();
        var ultimasCompras = await (
            from s in db.MovimientosStock.AsNoTracking()
            join pr in db.Proveedores on s.IdProveedor equals pr.IdProveedor
            where ids.Contains(s.IdProducto) && s.Cantidad > 0 && s.PrecioUnitarioCompra != null
            orderby s.IdStock descending
            select new
            {
                s.IdProducto,
                pr.Nombre,
                s.PrecioUnitarioCompra
            }).ToListAsync(ct);

        foreach (StockDisponibleRow row in lista)
        {
            var ultima = ultimasCompras.FirstOrDefault(x => x.IdProducto == row.IdProducto);
            if (ultima is not null)
            {
                row.UltimoProveedor = ultima.Nombre;
                row.UltimoPrecioCompra = ultima.PrecioUnitarioCompra;
            }
        }

        return lista;
    }

    public async Task<int> ObtenerStockMinimoActualAsync(int idProducto, CancellationToken ct = default)
    {
        return await db.MovimientosStock
            .Where(m => m.IdProducto == idProducto)
            .Select(m => (int?)m.StockMinimo)
            .MaxAsync(ct) ?? 0;
    }

    /// <summary>Devuelve datos para alerta si el producto maneja stock y la cantidad actual está en o bajo el mínimo.</summary>
    public async Task<StockCriticoVentaItem?> ObtenerItemAlertaSiStockEnMinimoAsync(int idProducto, CancellationToken ct = default)
    {
        Producto? producto = await db.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto, ct);
        if (producto is null || !producto.ManejaStock)
        {
            return null;
        }

        int minimo = await ObtenerStockMinimoActualAsync(idProducto, ct);
        int actual = await ObtenerStockCantidadActualAsync(idProducto, ct);
        if (actual > minimo)
        {
            return null;
        }

        return new StockCriticoVentaItem
        {
            NombreProducto = producto.Nombre,
            CodigoBarras = producto.CodigoBarras,
            StockActual = actual,
            StockMinimo = minimo
        };
    }
}
