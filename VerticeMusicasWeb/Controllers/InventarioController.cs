using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Helpers;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class InventarioController(AppDbContext context, InventarioStockService inventarioStock) : Controller
{
    public async Task<IActionResult> Index(int? categoriaId, int? proveedorId, string? q)
    {
        await CargarCategoriasFiltroAsync(categoriaId);
        await CargarProveedoresFiltroAsync(proveedorId);
        List<InventarioMovimientoRow> movimientos = await inventarioStock.GetMovimientosAsync(categoriaId, q, proveedorId);
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

        if (!await context.Proveedores.AnyAsync())
        {
            TempData["Error"] = "Registra al menos un proveedor antes de registrar entradas de inventario.";
            return RedirectToAction("Index", "Proveedores");
        }

        await CargarProductosSelectAsync();
        await CargarProveedoresSelectAsync();
        return View(new RegistrarEntradaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrarEntradaViewModel model)
    {
        NormalizarPreciosDesdeFormulario(model);
        ModelState.Clear();
        TryValidateModel(model);

        if (!await context.Productos.AnyAsync(p => p.IdProducto == model.IdProducto))
        {
            ModelState.AddModelError(nameof(model.IdProducto), "El producto seleccionado no es valido.");
        }

        if (!await context.Proveedores.AnyAsync(p => p.IdProveedor == model.IdProveedor))
        {
            ModelState.AddModelError(nameof(model.IdProveedor), "El proveedor seleccionado no es valido.");
        }

        if (!ModelState.IsValid)
        {
            await CargarProductosSelectAsync(model.IdProducto);
            await CargarProveedoresSelectAsync(model.IdProveedor);
            return View(model);
        }

        try
        {
            EntradaInventarioResultado resultado = await inventarioStock.RegistrarEntradaInventarioAsync(
                model.Fecha,
                model.IdProducto,
                model.Cantidad,
                model.StockMinimo,
                model.IdProveedor,
                model.PrecioUnitarioCompra,
                model.PorcentajeMargenVenta,
                model.PrecioVentaSugerido,
                model.AplicarPrecioAlProducto);

            TempData["Success"] = "Entrada de inventario registrada correctamente.";
            if (resultado.PrecioProductoActualizado && resultado.NuevoPrecioProducto.HasValue)
            {
                string nombre = resultado.NombreProducto ?? "el producto";
                TempData["PrecioProductoActualizado"] =
                    $"El precio de venta de {nombre} fue actualizado a {resultado.NuevoPrecioProducto.Value:C0} (COP).";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"No se pudo registrar el movimiento: {ex.Message}";
            await CargarProductosSelectAsync(model.IdProducto);
            await CargarProveedoresSelectAsync(model.IdProveedor);
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

    private async Task CargarProveedoresSelectAsync(int? seleccionado = null)
    {
        var items = await context.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.IdProveedor, Texto = p.Nombre + " — " + (p.Celular ?? p.CorreoElectronico ?? p.Contacto) })
            .ToListAsync();
        ViewBag.Proveedores = new SelectList(items, "IdProveedor", "Texto", seleccionado);
    }

    private async Task CargarProveedoresFiltroAsync(int? seleccionado)
    {
        List<Proveedor> proveedores = await context.Proveedores.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = "Todos los proveedores", Selected = !seleccionado.HasValue }
        };
        foreach (Proveedor p in proveedores)
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

    private void NormalizarPreciosDesdeFormulario(RegistrarEntradaViewModel model)
    {
        if (NumeroColombianoHelper.TryParsePrecio(Request.Form[nameof(model.PrecioUnitarioCompra)], out decimal compra))
        {
            model.PrecioUnitarioCompra = compra;
        }

        if (NumeroColombianoHelper.TryParsePorcentaje(Request.Form[nameof(model.PorcentajeMargenVenta)], out decimal margen))
        {
            model.PorcentajeMargenVenta = margen;
        }

        if (NumeroColombianoHelper.TryParsePrecio(Request.Form[nameof(model.PrecioVentaSugerido)], out decimal venta))
        {
            model.PrecioVentaSugerido = venta;
        }
        else if (model.PrecioUnitarioCompra > 0 && model.PorcentajeMargenVenta > 0)
        {
            model.PrecioVentaSugerido = Math.Round(
                model.PrecioUnitarioCompra + model.PrecioUnitarioCompra * model.PorcentajeMargenVenta / 100m,
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
