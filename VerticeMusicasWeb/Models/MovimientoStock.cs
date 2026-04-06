using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VerticeMusicasWeb.Models;

public class MovimientoStock
{
    [Key]
    public int IdStock { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [Display(Name = "Inventario")]
    public int IdInventario { get; set; }

    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [ForeignKey(nameof(IdInventario))]
    public Inventario? Inventario { get; set; }

    [ForeignKey(nameof(IdProducto))]
    public Producto? Producto { get; set; }
}
