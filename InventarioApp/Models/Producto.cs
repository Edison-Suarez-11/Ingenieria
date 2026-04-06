namespace InventarioApp.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Marca { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
}
