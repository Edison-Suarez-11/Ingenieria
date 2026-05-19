using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class VentasController(VentaService ventaService) : Controller
{
    public async Task<IActionResult> Historial()
    {
        VentaHistorialViewModel vm = await ventaService.ObtenerHistorialVentasAsync();
        return View(vm);
    }

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
            VentaRegistroResultado resultado = await ventaService.RegistrarVentaAsync(model);
            TempData["Success"] = $"Venta #{resultado.IdVenta} registrada correctamente.";
            if (resultado.StockCriticoItems.Count > 0)
            {
                TempData["StockCriticoJson"] = JsonSerializer.Serialize(resultado.StockCriticoItems);
            }

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
