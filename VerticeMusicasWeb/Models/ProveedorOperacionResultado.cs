namespace VerticeMusicasWeb.Models;

public class ProveedorOperacionResultado
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public Proveedor? Proveedor { get; set; }
    public Dictionary<string, string[]> Errores { get; set; } = new();
}
