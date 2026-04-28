using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class Venta
{
    [Key]
    public int IdVenta { get; set; }

    [Required]
    [MaxLength(32)]
    [Display(Name = "Fecha")]
    public string Fecha { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
    [Display(Name = "Total")]
    public decimal Total { get; set; }

    [Required]
    [MaxLength(40)]
    [Display(Name = "Metodo de pago")]
    public string MetodoPago { get; set; } = string.Empty;

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
