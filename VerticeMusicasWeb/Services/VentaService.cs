using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class VentaService(AppDbContext db, InventarioStockService inventarioStock)
{
    private static string FechaToTexto(DateTime fecha) =>
        fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

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

        return DateTime.TryParse(fechaTexto, out DateTime fechaFallback)
            ? fechaFallback
            : DateTime.MinValue;
    }

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
                EF.Functions.Like(p.CodigoBarras, like) ||
                (p.Marca != null && EF.Functions.Like(p.Marca, like)));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoVentaLookup
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                Marca = p.Marca ?? string.Empty,
                Precio = p.Precio,
                ManejaStock = p.ManejaStock,
                StockActual = p.MovimientosStock.Sum(m => (int?)m.Cantidad) ?? 0,
                StockMinimo = p.MovimientosStock.Select(m => (int?)m.StockMinimo).Max() ?? 0
            })
            .ToListAsync(ct);
    }

    public async Task<VentaRegistroResultado> RegistrarVentaAsync(RegistrarVentaViewModel model, CancellationToken ct = default)
    {
        List<RegistrarVentaItemViewModel> lineas = model.Items
            .Where(i => i.IdProducto > 0 && i.Cantidad > 0)
            .ToList();

        if (lineas.Count == 0)
        {
            throw new InvalidOperationException("Debes agregar al menos un producto.");
        }

        if (string.IsNullOrWhiteSpace(model.MetodoPago))
        {
            throw new InvalidOperationException("Debes seleccionar un metodo de pago.");
        }

        foreach (RegistrarVentaItemViewModel linea in lineas)
        {
            if (linea.PrecioUnitario <= 0)
            {
                throw new InvalidOperationException(
                    $"El precio unitario del producto «{linea.NombreProducto}» debe ser mayor a cero.");
            }
        }

        HashSet<int> idsProducto = lineas.Select(l => l.IdProducto).ToHashSet();
        List<Producto> productos = await db.Productos
            .Where(p => idsProducto.Contains(p.IdProducto))
            .ToListAsync(ct);

        if (productos.Count != idsProducto.Count)
        {
            throw new InvalidOperationException("Uno o mas productos del carrito no existen.");
        }

        decimal total = lineas.Sum(l => l.PrecioUnitario * l.Cantidad);

        var resultado = new VentaRegistroResultado();
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

            foreach (RegistrarVentaItemViewModel linea in lineas)
            {
                db.DetallesVenta.Add(new DetalleVenta
                {
                    IdVenta = venta.IdVenta,
                    IdProducto = linea.IdProducto,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario
                });
            }

            await db.SaveChangesAsync(ct);

            Dictionary<int, int> cantidadesPorProducto = lineas
                .GroupBy(l => l.IdProducto)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));

            foreach ((int idProducto, int cantidadVendida) in cantidadesPorProducto)
            {
                Producto producto = productos.First(p => p.IdProducto == idProducto);
                if (!producto.ManejaStock)
                {
                    continue;
                }

                int stockMinimo = await inventarioStock.ObtenerStockMinimoActualAsync(idProducto, ct);
                int stockActual = await inventarioStock.ObtenerStockCantidadActualAsync(idProducto, ct);
                int stockDespues = stockActual - cantidadVendida;

                await inventarioStock.RegistrarMovimientoAsync(DateTime.Now, idProducto, -cantidadVendida, stockMinimo, ct);

                if (stockDespues <= 0)
                {
                    resultado.StockCriticoItems.Add(new StockCriticoVentaItem
                    {
                        NombreProducto = producto.Nombre,
                        CodigoBarras = producto.CodigoBarras,
                        StockActual = stockDespues,
                        StockMinimo = stockMinimo
                    });
                }
            }

            await tx.CommitAsync(ct);
            resultado.IdVenta = venta.IdVenta;
            return resultado;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<VentaHistorialViewModel> ObtenerHistorialVentasAsync(CancellationToken ct = default)
    {
        List<Venta> ventas = await db.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.IdVenta)
            .ToListAsync(ct);

        var vm = new VentaHistorialViewModel();

        foreach (Venta v in ventas)
        {
            var item = new VentaHistorialItem
            {
                IdVenta = v.IdVenta,
                Fecha = TextoToFecha(v.Fecha),
                MetodoPago = v.MetodoPago,
                Total = v.Total,
                CantidadProductos = v.Detalles.Sum(d => d.Cantidad)
            };

            foreach (DetalleVenta d in v.Detalles.OrderBy(d => d.IdDetalle))
            {
                item.Detalles.Add(new VentaHistorialDetalleItem
                {
                    NombreProducto = d.Producto?.Nombre ?? "Producto eliminado",
                    CodigoBarras = d.Producto?.CodigoBarras ?? "-",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });
            }

            vm.Ventas.Add(item);
        }

        return vm;
    }
}
