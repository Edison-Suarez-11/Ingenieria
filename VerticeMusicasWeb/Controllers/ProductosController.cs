using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;

namespace VerticeMusicasWeb.Controllers;

public class ProductosController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? categoriaId, string? q)
    {
        IQueryable<Producto> query = context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria);

        if (categoriaId.HasValue)
        {
            int cid = categoriaId.Value;
            query = query.Where(p => p.IdCategoria == cid);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            string normalized = q.Trim();
            string like = $"%{normalized}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, like) ||
                p.CodigoBarras == normalized ||
                EF.Functions.Like(p.CodigoBarras, like));
        }

        List<Producto> productos = await query.OrderByDescending(p => p.IdProducto).ToListAsync();

        List<Categoria> categorias = await context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync();
        var filtroCategorias = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todas", Selected = !categoriaId.HasValue }
        };
        foreach (Categoria c in categorias)
        {
            filtroCategorias.Add(new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.NombreCategoria,
                Selected = categoriaId == c.IdCategoria
            });
        }
        ViewBag.CategoriasFiltro = filtroCategorias;
        ViewBag.Term = q;

        return View(productos);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Producto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!await context.Categorias.AnyAsync())
        {
            ModelState.AddModelError(nameof(Producto.IdCategoria), "Debes crear al menos una categoria primero.");
        }

        producto.CodigoBarras = producto.CodigoBarras?.Trim() ?? string.Empty;
        if (await ExisteCodigoBarrasAsync(producto.CodigoBarras, null))
        {
            ModelState.AddModelError(nameof(Producto.CodigoBarras), "Ya existe un producto con ese codigo de barras.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }

        try
        {
            producto.Nombre = producto.Nombre.Trim();
            producto.Marca = string.IsNullOrWhiteSpace(producto.Marca) ? null : producto.Marca.Trim();
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Producto.CodigoBarras), "No se pudo guardar: verifica que el codigo de barras sea unico.");
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo guardar el producto.";
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        Producto? producto = await context.Productos.FindAsync(id);
        if (producto is null)
        {
            TempData["Error"] = "Producto no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        await CargarCategoriasAsync(producto.IdCategoria);
        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.IdProducto)
        {
            TempData["Error"] = "Producto invalido.";
            return RedirectToAction(nameof(Index));
        }

        producto.CodigoBarras = producto.CodigoBarras?.Trim() ?? string.Empty;
        if (await ExisteCodigoBarrasAsync(producto.CodigoBarras, producto.IdProducto))
        {
            ModelState.AddModelError(nameof(Producto.CodigoBarras), "Ya existe un producto con ese codigo de barras.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }

        try
        {
            producto.Nombre = producto.Nombre.Trim();
            producto.Marca = string.IsNullOrWhiteSpace(producto.Marca) ? null : producto.Marca.Trim();
            context.Productos.Update(producto);
            await context.SaveChangesAsync();
            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Producto.CodigoBarras), "No se pudo actualizar: verifica que el codigo de barras sea unico.");
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo actualizar el producto.";
            await CargarCategoriasAsync(producto.IdCategoria);
            return View(producto);
        }
    }

    private async Task CargarCategoriasAsync(int? seleccionada = null)
    {
        List<Categoria> categorias = await context.Categorias
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync();

        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.IdCategoria), nameof(Categoria.NombreCategoria), seleccionada);
    }

    private async Task<bool> ExisteCodigoBarrasAsync(string codigo, int? excluirId)
    {
        string c = codigo.Trim();
        return await context.Productos.AnyAsync(p =>
            p.CodigoBarras == c &&
            (!excluirId.HasValue || p.IdProducto != excluirId.Value));
    }
}
