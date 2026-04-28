namespace VerticeMusicasWeb.Models;

public class VentaRegistroResultado
{
    public int IdVenta { get; set; }
    public List<StockCriticoVentaItem> StockCriticoItems { get; set; } = new();
}

public class StockCriticoVentaItem
{
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public bool DebajoDelMinimo => StockActual < StockMinimo;
}
