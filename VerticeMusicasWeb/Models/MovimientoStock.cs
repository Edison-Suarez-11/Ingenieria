using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VerticeMusicasWeb.Models;

public class MovimientoStock
{
    [Key]
    public int IdStock { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock minimo no puede ser negativo.")]
    [Display(Name = "Stock minimo")]
    public int StockMinimo { get; set; }

    [Display(Name = "Inventario")]
    public int IdInventario { get; set; }

    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [ForeignKey(nameof(IdInventario))]
    public Inventario? Inventario { get; set; }

    [ForeignKey(nameof(IdProducto))]
    public Producto? Producto { get; set; }
}
