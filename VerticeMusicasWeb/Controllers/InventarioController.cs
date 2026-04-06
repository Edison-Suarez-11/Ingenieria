using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class InventarioController(AppDbContext context, InventarioStockService inventarioStock) : Controller
{
    public async Task<IActionResult> Index(int? categoriaId, string? q)
    {
        await CargarCategoriasFiltroAsync(categoriaId);
        List<InventarioMovimientoRow> movimientos = await inventarioStock.GetMovimientosAsync(categoriaId, q);
        ViewBag.Term = q;
        return View(movimientos);
    }

    public async Task<IActionResult> Create()
    {
        if (!await context.Productos.AnyAsync())
        {
            TempData["Error"] = "Crea al menos un producto antes de registrar entradas de inventario.";
            return RedirectToAction(nameof(Index));
        }

        await CargarProductosSelectAsync();
        return View(new RegistrarEntradaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrarEntradaViewModel model)
    {
        if (!await context.Productos.AnyAsync(p => p.IdProducto == model.IdProducto))
        {
            ModelState.AddModelError(nameof(model.IdProducto), "El producto seleccionado no es valido.");
        }

        if (!ModelState.IsValid)
        {
            await CargarProductosSelectAsync(model.IdProducto);
            return View(model);
        }

        try
        {
            await inventarioStock.RegistrarMovimientoAsync(model.Fecha, model.IdProducto, model.Cantidad);
            TempData["Success"] = "Entrada de inventario registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"No se pudo registrar el movimiento: {ex.Message}";
            await CargarProductosSelectAsync(model.IdProducto);
            return View(model);
        }
    }

    private async Task CargarProductosSelectAsync(int? seleccionado = null)
    {
        var items = await context.Productos
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.IdProducto, Texto = p.Nombre + " — " + p.CodigoBarras })
            .ToListAsync();
        ViewBag.Productos = new SelectList(items, "IdProducto", "Texto", seleccionado);
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
