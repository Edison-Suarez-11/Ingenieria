using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Services;

public class InformesPdfService
{
    private const int MaxFilasTabla = 80;
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CO");

    static InformesPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarPdf(InformesViewModel vm, string seccion)
    {
        string periodo = FormatearPeriodo(vm);
        string filtros = FormatearFiltros(vm);
        string generado = DateTime.Now.ToString("dd/MM/yyyy HH:mm", Cultura);
        string tituloInforme = InformesSeccion.ObtenerTitulo(seccion);

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(32);
                page.MarginVertical(28);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("Vertice Muisca — Informes").Bold().FontSize(16);
                    col.Item().PaddingTop(2).Text(tituloInforme).SemiBold().FontSize(12)
                        .FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(4).Text($"Generado: {generado}").FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Periodo: {periodo}").SemiBold();
                    if (!string.IsNullOrWhiteSpace(filtros))
                    {
                        col.Item().Text($"Filtros: {filtros}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    }
                });

                page.Content().PaddingVertical(8).Column(col =>
                    AgregarContenidoPorSeccion(col, seccion, vm));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Pagina ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void AgregarContenidoPorSeccion(
        ColumnDescriptor col,
        string seccion,
        InformesViewModel vm)
    {
        if (InformesSeccion.EsCompleto(seccion))
        {
            col.Item().Element(c => SeccionResumen(c, vm.Resumen));
            col.Item().PaddingTop(12).Element(c => SeccionVentas(c, vm.Ventas));
            col.Item().PaddingTop(10).Element(c => SeccionMasVendidos(c, vm.ProductosMasVendidos));
            col.Item().PaddingTop(10).Element(c => SeccionEntradas(c, vm.EntradasInventario));
            col.Item().PaddingTop(10).Element(c => SeccionSalidas(c, vm.SalidasInventario));
            col.Item().PaddingTop(10).Element(c => SeccionComprasProveedor(c, vm.ComprasPorProveedor));
            col.Item().PaddingTop(10).Element(c => SeccionResumenProveedores(c, vm.Proveedores));
            col.Item().PaddingTop(10).Element(c => SeccionComparacion(c, vm.ComparacionPreciosProveedores));
            col.Item().PaddingTop(10).Element(c => SeccionCategorias(c, vm.Categorias));
            col.Item().PaddingTop(10).Element(c => SeccionProductos(c, vm.ProductosCatalogo));
            return;
        }

        switch (seccion)
        {
            case InformesSeccion.Ventas:
                col.Item().Element(c => SeccionVentas(c, vm.Ventas));
                break;
            case InformesSeccion.MasVendidos:
                col.Item().Element(c => SeccionMasVendidos(c, vm.ProductosMasVendidos));
                break;
            case InformesSeccion.Salidas:
                col.Item().Element(c => SeccionSalidas(c, vm.SalidasInventario));
                break;
            case InformesSeccion.Entradas:
                col.Item().Element(c => SeccionEntradas(c, vm.EntradasInventario));
                break;
            case InformesSeccion.ComprasProveedor:
                col.Item().Element(c => SeccionComprasProveedor(c, vm.ComprasPorProveedor));
                break;
            case InformesSeccion.Comparacion:
                col.Item().Element(c => SeccionComparacion(c, vm.ComparacionPreciosProveedores));
                break;
            case InformesSeccion.Proveedores:
                col.Item().Element(c => SeccionResumenProveedores(c, vm.Proveedores));
                break;
            case InformesSeccion.Productos:
                col.Item().Element(c => SeccionProductos(c, vm.ProductosCatalogo));
                break;
            case InformesSeccion.Categorias:
                col.Item().Element(c => SeccionCategorias(c, vm.Categorias));
                break;
            default:
                col.Item().Element(c => SeccionVentas(c, vm.Ventas));
                break;
        }
    }

    private static void SeccionResumen(IContainer container, InformesResumen r)
    {
        container.Column(col =>
        {
            col.Item().Text("Resumen ejecutivo").Bold().FontSize(11);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                FilaKpi(table, "Ventas", $"{r.TotalVentas} ({Dinero(r.MontoTotalVentas)})");
                FilaKpi(table, "Compras", $"{r.EntradasInventario} entradas · {Dinero(r.MontoTotalCompras)}");
                FilaKpi(table, "Unid. vendidas", r.UnidadesVendidas.ToString(Cultura));
                FilaKpi(table, "Productos", r.TotalProductos.ToString(Cultura));
                FilaKpi(table, "Categorias", r.TotalCategorias.ToString(Cultura));
                FilaKpi(table, "Proveedores", r.TotalProveedores.ToString(Cultura));
            });
        });
    }

    private static void FilaKpi(TableDescriptor table, string etiqueta, string valor)
    {
        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
            .Column(c =>
            {
                c.Item().Text(etiqueta).FontSize(7).FontColor(Colors.Grey.Darken1);
                c.Item().Text(valor).SemiBold();
            });
    }

    private static void SeccionVentas(IContainer container, List<InformeVentaFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Ventas", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "#", "Fecha", "Pago", "Total", "Uds.", "Detalle");
            foreach (InformeVentaFila v in filas.Take(MaxFilasTabla))
            {
                table.Cell().Text(v.IdVenta.ToString(Cultura));
                table.Cell().Text(Fecha(v.Fecha));
                table.Cell().Text(v.MetodoPago);
                table.Cell().AlignRight().Text(Dinero(v.Total));
                table.Cell().AlignRight().Text(v.Unidades.ToString(Cultura));
                table.Cell().Text(Truncar(v.DetalleProductos, 40));
            }
            });
            if (filas.Count > MaxFilasTabla)
            {
                col.Item().Element(c => NotaTruncado(c, filas.Count));
            }
        });
    }

    private static void SeccionMasVendidos(IContainer container, List<InformeProductoVendidoFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Productos mas vendidos", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "#", "Producto", "Codigo", "Unidades", "Monto");
            int rank = 1;
            foreach (InformeProductoVendidoFila p in filas.Take(30))
            {
                table.Cell().Text(rank.ToString(Cultura));
                table.Cell().Text(p.NombreProducto);
                table.Cell().Text(p.CodigoBarras);
                table.Cell().AlignRight().Text(p.UnidadesVendidas.ToString(Cultura));
                table.Cell().AlignRight().Text(Dinero(p.MontoTotal));
                rank++;
            }
            });
        });
    }

    private static void SeccionEntradas(IContainer container, List<InformeEntradaInventarioFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Entradas de inventario", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "Fecha", "Producto", "Proveedor", "Cant.", "P. compra", "Total");
            foreach (InformeEntradaInventarioFila e in filas.Take(MaxFilasTabla))
            {
                table.Cell().Text(Fecha(e.Fecha));
                table.Cell().Text(Truncar(e.NombreProducto, 28));
                table.Cell().Text(e.NombreProveedor ?? "—");
                table.Cell().AlignRight().Text(e.Cantidad.ToString(Cultura));
                table.Cell().AlignRight().Text(e.PrecioUnitarioCompra.HasValue ? Dinero(e.PrecioUnitarioCompra.Value) : "—");
                table.Cell().AlignRight().Text(Dinero(e.TotalCompra));
            }
            });
            if (filas.Count > MaxFilasTabla)
            {
                col.Item().Element(c => NotaTruncado(c, filas.Count));
            }
        });
    }

    private static void SeccionSalidas(IContainer container, List<InformeSalidaInventarioFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Salidas de inventario", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "Fecha", "Producto", "Categoria", "Cant.", "Motivo");
            foreach (InformeSalidaInventarioFila s in filas.Take(MaxFilasTabla))
            {
                table.Cell().Text(Fecha(s.Fecha));
                table.Cell().Text(Truncar(s.NombreProducto, 28));
                table.Cell().Text(s.NombreCategoria);
                table.Cell().AlignRight().Text(s.Cantidad.ToString(Cultura));
                table.Cell().Text(Truncar(s.Motivo, 35));
            }
            });
            if (filas.Count > MaxFilasTabla)
            {
                col.Item().Element(c => NotaTruncado(c, filas.Count));
            }
        });
    }

    private static void SeccionComprasProveedor(IContainer container, List<InformeCompraProveedorFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Compras por proveedor (detalle)", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "Fecha", "Proveedor", "Producto", "Cant.", "P. unit.", "Total");
            foreach (InformeCompraProveedorFila c in filas.Take(MaxFilasTabla))
            {
                table.Cell().Text(Fecha(c.Fecha));
                table.Cell().Text(Truncar(c.NombreProveedor, 22));
                table.Cell().Text(Truncar(c.NombreProducto, 22));
                table.Cell().AlignRight().Text(c.Cantidad.ToString(Cultura));
                table.Cell().AlignRight().Text(Dinero(c.PrecioUnitario));
                table.Cell().AlignRight().Text(Dinero(c.TotalLinea));
            }
            });
            if (filas.Count > MaxFilasTabla)
            {
                col.Item().Element(c => NotaTruncado(c, filas.Count));
            }
        });
    }

    private static void SeccionResumenProveedores(IContainer container, List<InformeProveedorResumenFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Resumen por proveedor", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "Proveedor", "Contacto", "Compras", "Uds.", "Productos", "Monto");
            foreach (InformeProveedorResumenFila p in filas.Take(MaxFilasTabla))
            {
                table.Cell().Text(p.NombreProveedor);
                table.Cell().Text(Truncar(p.Contacto, 25));
                table.Cell().AlignRight().Text(p.NumeroCompras.ToString(Cultura));
                table.Cell().AlignRight().Text(p.UnidadesCompradas.ToString(Cultura));
                table.Cell().AlignRight().Text(p.ProductosDistintos.ToString(Cultura));
                table.Cell().AlignRight().Text(Dinero(p.MontoTotalCompras));
            }
            });
        });
    }

    private static void SeccionComparacion(IContainer container, List<InformeComparacionPrecioFila> items)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Comparacion de precios por proveedor", items.Count));
            if (items.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Column(inner =>
        {
            foreach (InformeComparacionPrecioFila item in items.Take(15))
            {
                inner.Item().PaddingTop(6).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(block =>
                {
                    block.Item().Text($"{item.NombreProducto} ({item.CodigoBarras})").SemiBold();
                    block.Item().Text($"Venta: {Dinero(item.PrecioVenta)} · Mejor compra: {(item.PrecioCompraMasBajo.HasValue ? Dinero(item.PrecioCompraMasBajo.Value) + " · " + item.ProveedorMasEconomico : "—")}")
                        .FontSize(8);
                    block.Item().PaddingTop(4).Table(table =>
                    {
                        Encabezado(table, "Proveedor", "Ultimo", "Min", "Max", "Prom.", "N°");
                        foreach (InformePrecioProveedorItem pr in item.PreciosPorProveedor)
                        {
                            table.Cell().Text(pr.NombreProveedor);
                            table.Cell().AlignRight().Text(pr.UltimoPrecio.HasValue ? Dinero(pr.UltimoPrecio.Value) : "—");
                            table.Cell().AlignRight().Text(pr.PrecioMinimo.HasValue ? Dinero(pr.PrecioMinimo.Value) : "—");
                            table.Cell().AlignRight().Text(pr.PrecioMaximo.HasValue ? Dinero(pr.PrecioMaximo.Value) : "—");
                            table.Cell().AlignRight().Text(pr.PrecioPromedio.HasValue ? Dinero(pr.PrecioPromedio.Value) : "—");
                            table.Cell().AlignRight().Text(pr.NumeroCompras.ToString(Cultura));
                        }
                    });
                });
            }
            if (items.Count > 15)
            {
                inner.Item().PaddingTop(4).Text($"(Se muestran 15 de {items.Count} productos comparados)").Italic().FontSize(7);
            }
            });
        });
    }

    private static void SeccionProductos(IContainer container, List<InformeProductoCatalogoFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Catalogo de productos", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
            {
                Encabezado(table, "Producto", "Codigo", "Categoria", "P. venta", "Stock", "Mejor prov.");
                foreach (InformeProductoCatalogoFila p in filas.Take(MaxFilasTabla))
                {
                    table.Cell().Text(Truncar(p.NombreProducto, 24));
                    table.Cell().Text(p.CodigoBarras);
                    table.Cell().Text(p.NombreCategoria);
                    table.Cell().AlignRight().Text(Dinero(p.PrecioVenta));
                    table.Cell().AlignRight().Text(p.StockDisponible.ToString(Cultura));
                    table.Cell().Text(p.MejorProveedor ?? "—");
                }
            });
            if (filas.Count > MaxFilasTabla)
            {
                col.Item().Element(c => NotaTruncado(c, filas.Count));
            }
        });
    }

    private static void SeccionCategorias(IContainer container, List<InformeCategoriaFila> filas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Categorias", filas.Count));
            if (filas.Count == 0)
            {
                col.Item().Element(SinDatos);
                return;
            }

            col.Item().Table(table =>
        {
            Encabezado(table, "Categoria", "Productos", "Stock total");
            foreach (InformeCategoriaFila c in filas)
            {
                table.Cell().Text(c.NombreCategoria);
                table.Cell().AlignRight().Text(c.CantidadProductos.ToString(Cultura));
                table.Cell().AlignRight().Text(c.StockTotal.ToString(Cultura));
            }
            });
        });
    }

    private static void TituloSeccion(IContainer container, string titulo, int count) =>
        container.Text($"{titulo} ({count})").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);

    private static void SinDatos(IContainer container) =>
        container.PaddingTop(4).Text("Sin datos para el periodo o filtros seleccionados.")
            .Italic().FontColor(Colors.Grey.Medium);

    private static void Encabezado(TableDescriptor table, params string[] columnas)
    {
        table.ColumnsDefinition(c =>
        {
            foreach (string _ in columnas)
            {
                c.RelativeColumn();
            }
        });
        foreach (string col in columnas)
        {
            table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                .Text(col).SemiBold().FontSize(8);
        }
    }

    private static void NotaTruncado(IContainer container, int total)
    {
        if (total > MaxFilasTabla)
        {
            container.PaddingTop(4).Text($"Mostrando las primeras {MaxFilasTabla} de {total} filas.")
                .FontSize(7).Italic().FontColor(Colors.Grey.Darken1);
        }
    }

    private static string FormatearPeriodo(InformesViewModel vm)
    {
        if (vm.Desde.HasValue && vm.Hasta.HasValue)
        {
            return $"{vm.Desde.Value:dd/MM/yyyy} — {vm.Hasta.Value:dd/MM/yyyy}";
        }
        if (vm.Desde.HasValue)
        {
            return $"Desde {vm.Desde.Value:dd/MM/yyyy}";
        }
        if (vm.Hasta.HasValue)
        {
            return $"Hasta {vm.Hasta.Value:dd/MM/yyyy}";
        }
        return "Todo el historial";
    }

    private static string FormatearFiltros(InformesViewModel vm)
    {
        var partes = new List<string>();
        if (vm.IdProveedor.HasValue)
        {
            partes.Add($"proveedor id={vm.IdProveedor}");
        }
        if (vm.IdCategoria.HasValue)
        {
            partes.Add($"categoria id={vm.IdCategoria}");
        }
        if (vm.IdProductoComparar.HasValue)
        {
            partes.Add($"producto comparar id={vm.IdProductoComparar}");
        }
        if (!string.IsNullOrWhiteSpace(vm.TerminoBusqueda))
        {
            partes.Add($"busqueda=\"{vm.TerminoBusqueda}\"");
        }
        return string.Join("; ", partes);
    }

    private static string Dinero(decimal m) => m.ToString("C0", Cultura);

    private static string Fecha(DateTime f) =>
        f == DateTime.MinValue ? "—" : f.ToString("dd/MM/yyyy", Cultura);

    private static string Truncar(string? texto, int max) =>
        string.IsNullOrEmpty(texto) ? "—" :
        texto.Length <= max ? texto : texto[..(max - 1)] + "…";
}
