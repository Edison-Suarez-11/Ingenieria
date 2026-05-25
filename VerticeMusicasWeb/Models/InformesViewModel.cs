namespace VerticeMusicasWeb.Models;

public class InformesViewModel
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdProductoComparar { get; set; }
    public int? IdProveedor { get; set; }
    public int? IdCategoria { get; set; }
    public string? TerminoBusqueda { get; set; }

    public InformesResumen Resumen { get; set; } = new();
    public List<InformeVentaFila> Ventas { get; set; } = [];
    public List<InformeEntradaInventarioFila> EntradasInventario { get; set; } = [];
    public List<InformeSalidaInventarioFila> SalidasInventario { get; set; } = [];
    public List<InformeProductoVendidoFila> ProductosMasVendidos { get; set; } = [];
    public List<InformeProductoCatalogoFila> ProductosCatalogo { get; set; } = [];
    public List<InformeCategoriaFila> Categorias { get; set; } = [];
    public List<InformeProveedorResumenFila> Proveedores { get; set; } = [];
    public List<InformeCompraProveedorFila> ComprasPorProveedor { get; set; } = [];
    public List<InformeComparacionPrecioFila> ComparacionPreciosProveedores { get; set; } = [];
}

public class InformesResumen
{
    public int TotalVentas { get; set; }
    public decimal MontoTotalVentas { get; set; }
    public int UnidadesVendidas { get; set; }
    public int EntradasInventario { get; set; }
    public int UnidadesIngresadas { get; set; }
    public int SalidasInventario { get; set; }
    public int UnidadesRetiradas { get; set; }
    public int TotalProductos { get; set; }
    public int TotalCategorias { get; set; }
    public int TotalProveedores { get; set; }
    public decimal MontoTotalCompras { get; set; }
}

public class InformeVentaFila
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadLineas { get; set; }
    public int Unidades { get; set; }
    public string DetalleProductos { get; set; } = string.Empty;
}

public class InformeEntradaInventarioFila
{
    public int IdInventario { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public string? NombreProveedor { get; set; }
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; }
    public decimal? PrecioUnitarioCompra { get; set; }
    public decimal TotalCompra => (PrecioUnitarioCompra ?? 0) * Cantidad;
}

public class InformeSalidaInventarioFila
{
    public int IdInventario { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class InformeProductoVendidoFila
{
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public int UnidadesVendidas { get; set; }
    public decimal MontoTotal { get; set; }
}

public class InformeProductoCatalogoFila
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockDisponible { get; set; }
    public string? MejorProveedor { get; set; }
    public decimal? MejorPrecioCompra { get; set; }
}

public class InformeCategoriaFila
{
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int CantidadProductos { get; set; }
    public int StockTotal { get; set; }
}

public class InformeProveedorResumenFila
{
    public int IdProveedor { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public int NumeroCompras { get; set; }
    public int UnidadesCompradas { get; set; }
    public decimal MontoTotalCompras { get; set; }
    public int ProductosDistintos { get; set; }
}

public class InformeCompraProveedorFila
{
    public DateTime Fecha { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal TotalLinea { get; set; }
}

public class InformeComparacionPrecioFila
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public List<InformePrecioProveedorItem> PreciosPorProveedor { get; set; } = [];
    public decimal? PrecioCompraMasBajo => PreciosPorProveedor
        .Where(x => x.UltimoPrecio.HasValue)
        .Select(x => x.UltimoPrecio)
        .Min();
    public string? ProveedorMasEconomico => PreciosPorProveedor
        .Where(x => x.UltimoPrecio == PrecioCompraMasBajo)
        .Select(x => x.NombreProveedor)
        .FirstOrDefault();
}

public class InformePrecioProveedorItem
{
    public int IdProveedor { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public decimal? PrecioPromedio { get; set; }
    public decimal? UltimoPrecio { get; set; }
    public DateTime? FechaUltimaCompra { get; set; }
    public int NumeroCompras { get; set; }
}
