using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class ConsultaStockController(AppDbContext context, InventarioStockService inventarioStock) : Controller
{
    public async Task<IActionResult> Index(int? categoriaId, int? proveedorId, string? q)
    {
        await CargarCategoriasFiltroAsync(categoriaId);
        await CargarProveedoresFiltroAsync(proveedorId);
        List<StockDisponibleRow> filas = await inventarioStock.GetStockDisponibleAsync(categoriaId, q, proveedorId);
        ViewBag.Term = q;
        return View(filas);
    }

    private async Task CargarProveedoresFiltroAsync(int? seleccionado)
    {
        var proveedores = await context.Proveedores.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todos los proveedores", Selected = !seleccionado.HasValue }
        };
        foreach (var p in proveedores)
        {
            items.Add(new SelectListItem
            {
                Value = p.IdProveedor.ToString(),
                Text = p.Nombre,
                Selected = seleccionado == p.IdProveedor
            });
        }
        ViewBag.ProveedoresFiltro = items;
    }

    private async Task CargarCategoriasFiltroAsync(int? seleccionada)
    {
        List<Categoria> categorias = await context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync();
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todas", Selected = !seleccionada.HasValue }
        };
        foreach (Categoria c in categorias)
        {
            items.Add(new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.NombreCategoria,
                Selected = seleccionada == c.IdCategoria
            });
        }
        ViewBag.CategoriasFiltro = items;
    }
}
