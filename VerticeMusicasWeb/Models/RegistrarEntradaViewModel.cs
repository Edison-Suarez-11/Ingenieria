using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class RegistrarEntradaViewModel
{
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto.")]
    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;
}
