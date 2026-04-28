namespace VerticeMusicasWeb.Models;

public class StockDisponibleRow
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int StockMinimo { get; set; }
    public bool ManejaStock { get; set; }
    public bool AlertaStockMinimo => ManejaStock && CantidadDisponible <= StockMinimo;
    public bool AlertaStockCritico => ManejaStock && CantidadDisponible <= 0;
}
