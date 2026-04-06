using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class Inventario
{
    [Key]
    public int IdInventario { get; set; }

    [Required]
    [MaxLength(32)]
    [Display(Name = "Fecha")]
    public string Fecha { get; set; } = string.Empty;

    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
}
