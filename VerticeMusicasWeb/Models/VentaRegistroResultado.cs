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
    /// <summary>Tras la venta el inventario quedó en cero o negativo.</summary>
    public bool StockEnCeroONegativo => StockActual <= 0;
    public bool DebajoDelMinimo => StockActual < StockMinimo;
}
