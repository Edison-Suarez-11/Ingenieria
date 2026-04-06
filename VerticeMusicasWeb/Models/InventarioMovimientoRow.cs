namespace VerticeMusicasWeb.Models;

public class InventarioMovimientoRow
{
    public int IdInventario { get; set; }
    public DateTime Fecha { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
