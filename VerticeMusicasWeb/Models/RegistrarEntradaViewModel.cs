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

    [Range(0, int.MaxValue, ErrorMessage = "El stock minimo no puede ser negativo.")]
    [Display(Name = "Stock minimo")]
    public int StockMinimo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El precio de compra es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor a cero.")]
    [Display(Name = "Precio unitario de compra")]
    public decimal PrecioUnitarioCompra { get; set; }

    [Required(ErrorMessage = "El porcentaje de margen es obligatorio.")]
    [Range(0.01, 999.99, ErrorMessage = "El porcentaje de margen debe ser mayor a cero.")]
    [Display(Name = "Porcentaje de margen de venta (%)")]
    public decimal PorcentajeMargenVenta { get; set; } = 30;

    [Required(ErrorMessage = "El precio de venta sugerido es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta sugerido debe ser mayor a cero.")]
    [Display(Name = "Precio de venta sugerido")]
    public decimal PrecioVentaSugerido { get; set; }

    [Display(Name = "Aplicar este precio de venta al producto")]
    public bool AplicarPrecioAlProducto { get; set; }
}
