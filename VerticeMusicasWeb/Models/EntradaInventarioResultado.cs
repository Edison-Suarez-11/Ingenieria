namespace VerticeMusicasWeb.Models;

public class EntradaInventarioResultado
{
    public int IdInventario { get; set; }
    public bool PrecioProductoActualizado { get; set; }
    public decimal? NuevoPrecioProducto { get; set; }
    public string? NombreProducto { get; set; }
}
