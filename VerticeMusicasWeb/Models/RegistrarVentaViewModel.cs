using System.ComponentModel.DataAnnotations;

namespace VerticeMusicasWeb.Models;

public class RegistrarVentaViewModel
{
    [Required(ErrorMessage = "Selecciona un metodo de pago.")]
    [Display(Name = "Metodo de pago")]
    public string MetodoPago { get; set; } = "Efectivo";

    [MinLength(1, ErrorMessage = "Agrega al menos un producto al carrito.")]
    public List<RegistrarVentaItemViewModel> Items { get; set; } = [];
}

public class RegistrarVentaItemViewModel
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public int Cantidad { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a cero.")]
    public decimal PrecioUnitario { get; set; }
}

public class ProductoVentaLookup
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool ManejaStock { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
}
