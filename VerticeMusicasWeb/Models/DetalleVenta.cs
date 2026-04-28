using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VerticeMusicasWeb.Models;

public class DetalleVenta
{
    [Key]
    public int IdDetalle { get; set; }

    [Required]
    [Display(Name = "Venta")]
    public int IdVenta { get; set; }

    [Required]
    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    [Display(Name = "Precio unitario")]
    public decimal PrecioUnitario { get; set; }

    [ForeignKey(nameof(IdVenta))]
    public Venta? Venta { get; set; }

    [ForeignKey(nameof(IdProducto))]
    public Producto? Producto { get; set; }
}
