using InventarioApp.Data;
using InventarioApp.Models;

namespace InventarioApp.Services;

public class InventarioService
{
    private readonly StockService stockService = new();

    public int RegistrarInventarioInicial(int idProducto, int cantidad, DateTime fecha)
    {
        return RegistrarMovimiento(idProducto, cantidad, fecha);
    }

    public int RegistrarEntradaInventario(int idProducto, int cantidad, DateTime fecha)
    {
        return RegistrarMovimiento(idProducto, cantidad, fecha);
    }

    public List<InventarioMovimiento> ListarMovimientos(int? idCategoria, string? terminoBusqueda)
    {
        return Database.GetInventarioMovimientos(idCategoria, terminoBusqueda);
    }

    public int ObtenerStockActual(int idProducto)
    {
        return stockService.ObtenerStockActualPorProducto(idProducto);
    }

    private static void ValidarMovimiento(int idProducto, int cantidad)
    {
        if (idProducto <= 0)
            throw new InvalidOperationException("Debe seleccionar un producto válido.");

        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a 0.");

        if (!Database.ExisteProductoPorId(idProducto))
            throw new InvalidOperationException("El producto seleccionado no existe.");
    }

    private int RegistrarMovimiento(int idProducto, int cantidad, DateTime fecha)
    {
        ValidarMovimiento(idProducto, cantidad);
        return Database.RegistrarMovimientoInventario(fecha, idProducto, cantidad);
    }
}

