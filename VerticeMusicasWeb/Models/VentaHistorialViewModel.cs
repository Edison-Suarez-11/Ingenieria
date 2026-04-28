namespace VerticeMusicasWeb.Models;

public class VentaHistorialViewModel
{
    public List<VentaHistorialItem> Ventas { get; set; } = new();
}

public class VentaHistorialItem
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadProductos { get; set; }
    public List<VentaHistorialDetalleItem> Detalles { get; set; } = new();
}

public class VentaHistorialDetalleItem
{
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
