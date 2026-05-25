using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class InformesController(InformesService informes, InformesPdfService informesPdf, AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(
        DateTime? desde,
        DateTime? hasta,
        int? idProductoComparar,
        int? idProveedor,
        int? idCategoria,
        string? q)
    {
        InformesViewModel vm = await informes.ObtenerInformesAsync(
            desde, hasta, idProductoComparar, idProveedor, idCategoria, q);
        await CargarFiltrosAsync(idProductoComparar, idProveedor, idCategoria);
        ViewBag.Term = q;
        return View(vm);
    }

    public async Task<IActionResult> ExportPdf(
        DateTime? desde,
        DateTime? hasta,
        int? idProductoComparar,
        int? idProveedor,
        int? idCategoria,
        string? q,
        string? seccion)
    {
        string seccionNormalizada = InformesSeccion.Normalizar(seccion);

        InformesViewModel vm = await informes.ObtenerInformesAsync(
            desde, hasta, idProductoComparar, idProveedor, idCategoria, q);
        byte[] pdf = informesPdf.GenerarPdf(vm, seccionNormalizada);
        string slug = seccionNormalizada.Replace("-", "_");
        string nombre = $"Informe_{slug}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        return File(pdf, "application/pdf", nombre);
    }

    private async Task CargarFiltrosAsync(int? productoSel, int? proveedorSel, int? categoriaSel)
    {
        var productos = await context.Productos.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.IdProducto, Texto = p.Nombre + " — " + p.CodigoBarras })
            .ToListAsync();
        ViewBag.ProductosComparar = new SelectList(productos, "IdProducto", "Texto", productoSel);

        var proveedores = await context.Proveedores.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.IdProveedor, Texto = p.Nombre })
            .ToListAsync();
        var itemsProveedor = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todos los proveedores", Selected = !proveedorSel.HasValue }
        };
        foreach (var p in proveedores)
        {
            itemsProveedor.Add(new SelectListItem
            {
                Value = p.IdProveedor.ToString(),
                Text = p.Texto,
                Selected = proveedorSel == p.IdProveedor
            });
        }
        ViewBag.ProveedoresFiltro = itemsProveedor;

        var categorias = await context.Categorias.AsNoTracking()
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync();
        var itemsCategoria = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todas las categorias", Selected = !categoriaSel.HasValue }
        };
        foreach (Categoria c in categorias)
        {
            itemsCategoria.Add(new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.NombreCategoria,
                Selected = categoriaSel == c.IdCategoria
            });
        }
        ViewBag.CategoriasFiltro = itemsCategoria;
    }
}
