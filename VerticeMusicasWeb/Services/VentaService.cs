using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class VentaService(AppDbContext db, InventarioStockService inventarioStock)
{
    private static string FechaToTexto(DateTime fecha) =>
        fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public async Task<List<ProductoVentaLookup>> BuscarProductosVentaAsync(string? term, CancellationToken ct = default)
    {
        IQueryable<Producto> query = db.Productos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            string normalized = term.Trim();
            string like = $"%{normalized}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, like) ||
                p.CodigoBarras == normalized ||
                EF.Functions.Like(p.CodigoBarras, like));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoVentaLookup
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                Precio = p.Precio,
                ManejaStock = p.ManejaStock,
                StockActual = p.MovimientosStock.Sum(m => (int?)m.Cantidad) ?? 0,
                StockMinimo = p.MovimientosStock.Select(m => (int?)m.StockMinimo).Max() ?? 0
            })
            .ToListAsync(ct);
    }

    public async Task<int> RegistrarVentaAsync(RegistrarVentaViewModel model, CancellationToken ct = default)
    {
        if (model.Items.Count == 0)
        {
            throw new InvalidOperationException("Debes agregar al menos un producto.");
        }

        if (string.IsNullOrWhiteSpace(model.MetodoPago))
        {
            throw new InvalidOperationException("Debes seleccionar un metodo de pago.");
        }

        Dictionary<int, int> cantidadesPorProducto = model.Items
            .GroupBy(x => x.IdProducto)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

        List<Producto> productos = await db.Productos
            .Where(p => cantidadesPorProducto.Keys.Contains(p.IdProducto))
            .ToListAsync(ct);

        if (productos.Count != cantidadesPorProducto.Count)
        {
            throw new InvalidOperationException("Uno o mas productos del carrito no existen.");
        }

        decimal total = 0m;
        foreach (Producto p in productos)
        {
            int cantidad = cantidadesPorProducto[p.IdProducto];
            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
            }
            total += p.Precio * cantidad;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var venta = new Venta
            {
                Fecha = FechaToTexto(DateTime.Now),
                Total = total,
                MetodoPago = model.MetodoPago.Trim()
            };
            db.Ventas.Add(venta);
            await db.SaveChangesAsync(ct);

            foreach ((int idProducto, int cantidad) in cantidadesPorProducto)
            {
                Producto producto = productos.First(p => p.IdProducto == idProducto);
                db.DetallesVenta.Add(new DetalleVenta
                {
                    IdVenta = venta.IdVenta,
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio
                });
            }
            await db.SaveChangesAsync(ct);

            foreach ((int idProducto, int cantidad) in cantidadesPorProducto)
            {
                Producto producto = productos.First(p => p.IdProducto == idProducto);
                if (!producto.ManejaStock)
                {
                    continue;
                }

                int stockMinimo = await inventarioStock.ObtenerStockMinimoActualAsync(idProducto, ct);
                await inventarioStock.RegistrarMovimientoAsync(DateTime.Now, idProducto, -cantidad, stockMinimo, ct);
            }

            await tx.CommitAsync(ct);
            return venta.IdVenta;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
