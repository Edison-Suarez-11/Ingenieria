using InventarioApp.Data;
using InventarioApp.Models;

namespace InventarioApp.Services;

public class StockService
{
    public List<StockDisponible> ListarStockDisponible(int? idCategoria, string? terminoBusqueda)
    {
        return Database.GetStockDisponible(idCategoria, terminoBusqueda);
    }

    public int ObtenerStockActualPorProducto(int idProducto)
    {
        return Database.ObtenerStockCantidadActual(idProducto);
    }
}

