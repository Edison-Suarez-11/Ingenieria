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
        if (cantidad <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor a cero.");
        }

        bool existeProducto = await db.Productos.AnyAsync(p => p.IdProducto == idProducto, ct);
        if (!existeProducto)
        {
            throw new InvalidOperationException("El producto no existe.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var inv = new Inventario { Fecha = FechaToTexto(fecha) };
            db.Inventarios.Add(inv);
            await db.SaveChangesAsync(ct);

            db.MovimientosStock.Add(new MovimientoStock
            {
                Cantidad = cantidad,
                IdInventario = inv.IdInventario,
                IdProducto = idProducto
            });
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return inv.IdInventario;
        }
        catch
        {
            await tx.RollbackAsync(ct);
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

    public async Task<List<InventarioMovimientoRow>> GetMovimientosAsync(int? idCategoria, string? term, CancellationToken ct = default)
    {
        bool tieneTerm = !string.IsNullOrWhiteSpace(term);
        string? normalized = tieneTerm ? term!.Trim() : null;

        var query =
            from s in db.MovimientosStock
            join i in db.Inventarios on s.IdInventario equals i.IdInventario
            join p in db.Productos on s.IdProducto equals p.IdProducto
            join c in db.Categorias on p.IdCategoria equals c.IdCategoria
            select new { s, i, p, c };

        if (idCategoria.HasValue)
        {
            int cid = idCategoria.Value;
            query = query.Where(x => x.c.IdCategoria == cid);
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
                x.s.Cantidad
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
            Cantidad = x.Cantidad
        });
    }

    public async Task<List<StockDisponibleRow>> GetStockDisponibleAsync(int? idCategoria, string? term, CancellationToken ct = default)
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

        return await productos
            .Select(p => new StockDisponibleRow
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                NombreCategoria = p.Categoria!.NombreCategoria,
                CantidadDisponible = p.MovimientosStock.Sum(m => (int?)m.Cantidad) ?? 0
            })
            .Where(x => x.CantidadDisponible > 0)
            .OrderByDescending(x => x.CantidadDisponible)
            .ToListAsync(ct);
    }
}
