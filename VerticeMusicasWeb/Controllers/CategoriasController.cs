using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Controllers;

public class CategoriasController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        IQueryable<Categoria> query = context.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            string like = $"%{q.Trim()}%";
            query = query.Where(c => EF.Functions.Like(c.NombreCategoria, like));
        }

        List<Categoria> categorias = await query.OrderByDescending(c => c.IdCategoria).ToListAsync();
        ViewBag.Term = q;
        return View(categorias);
    }

    public IActionResult Create()
    {
        return View(new Categoria());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (await ExisteNombreCategoriaAsync(categoria.NombreCategoria, null))
        {
            ModelState.AddModelError(nameof(Categoria.NombreCategoria), "Ya existe una categoria con ese nombre.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        try
        {
            categoria.NombreCategoria = categoria.NombreCategoria.Trim();
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            TempData["Success"] = "Categoria creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo guardar la categoria.";
            return View(categoria);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        Categoria? categoria = await context.Categorias.FindAsync(id);
        if (categoria is null)
        {
            TempData["Error"] = "Categoria no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.IdCategoria)
        {
            TempData["Error"] = "Categoria invalida.";
            return RedirectToAction(nameof(Index));
        }

        if (await ExisteNombreCategoriaAsync(categoria.NombreCategoria, categoria.IdCategoria))
        {
            ModelState.AddModelError(nameof(Categoria.NombreCategoria), "Ya existe una categoria con ese nombre.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        try
        {
            categoria.NombreCategoria = categoria.NombreCategoria.Trim();
            context.Categorias.Update(categoria);
            await context.SaveChangesAsync();
            TempData["Success"] = "Categoria actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo actualizar la categoria.";
            return View(categoria);
        }
    }

    private async Task<bool> ExisteNombreCategoriaAsync(string nombre, int? excluirId)
    {
        string n = nombre.Trim().ToLowerInvariant();
        return await context.Categorias.AnyAsync(c =>
            c.NombreCategoria.ToLower() == n &&
            (!excluirId.HasValue || c.IdCategoria != excluirId.Value));
    }
}
