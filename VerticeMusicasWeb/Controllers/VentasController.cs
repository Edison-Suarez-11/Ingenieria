using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class VentasController(VentaService ventaService) : Controller
{
    public async Task<IActionResult> Create(string? q)
    {
        List<ProductoVentaLookup> productos = await ventaService.BuscarProductosVentaAsync(q);
        ViewBag.Term = q;
        ViewBag.ProductosJson = JsonSerializer.Serialize(productos);
        return View(new RegistrarVentaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrarVentaViewModel model)
    {
        model.Items = model.Items.Where(i => i.IdProducto > 0 && i.Cantidad > 0).ToList();

        if (!ModelState.IsValid)
        {
            List<ProductoVentaLookup> productosInvalid = await ventaService.BuscarProductosVentaAsync(null);
            ViewBag.ProductosJson = JsonSerializer.Serialize(productosInvalid);
            return View(model);
        }

        try
        {
            int idVenta = await ventaService.RegistrarVentaAsync(model);
            TempData["Success"] = $"Venta #{idVenta} registrada correctamente.";
            return RedirectToAction(nameof(Create));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            List<ProductoVentaLookup> productos = await ventaService.BuscarProductosVentaAsync(null);
            ViewBag.ProductosJson = JsonSerializer.Serialize(productos);
            return View(model);
        }
    }
}
