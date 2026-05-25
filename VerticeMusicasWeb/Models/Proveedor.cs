using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class Proveedor
{
    [Key]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contacto del proveedor es obligatorio.")]
    [Display(Name = "Contacto")]
    public string Contacto { get; set; } = string.Empty;

    public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
}
