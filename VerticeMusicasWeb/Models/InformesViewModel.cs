namespace VerticeMusicasWeb.Models;

public class InformesViewModel
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public InformesResumen Resumen { get; set; } = new();
    public List<InformeVentaFila> Ventas { get; set; } = [];
    public List<InformeEntradaInventarioFila> EntradasInventario { get; set; } = [];
    public List<InformeSalidaInventarioFila> SalidasInventario { get; set; } = [];
    public List<InformeProductoVendidoFila> ProductosMasVendidos { get; set; } = [];
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
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; }
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
