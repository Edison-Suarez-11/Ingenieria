namespace VerticeMusicasWeb.Models;

public class StockDisponibleRow
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
}
