using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VerticeMusicasWeb.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo de barras es obligatorio.")]
    [Display(Name = "Codigo de Barras")]
    public string CodigoBarras { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Display(Name = "Marca")]
    [MaxLength(120)]
    public string? Marca { get; set; }

    [Display(Name = "Maneja stock")]
    public bool ManejaStock { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoria.")]
    [Display(Name = "Categoria")]
    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
}
