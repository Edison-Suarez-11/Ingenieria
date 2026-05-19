using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class DashboardController(AppDbContext context, InventarioStockService inventarioStock) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.CantidadCategorias = await context.Categorias.CountAsync();
        ViewBag.CantidadProductos = await context.Productos.CountAsync();
        ViewBag.CantidadMovimientos = await context.MovimientosStock.CountAsync();
        ViewBag.CantidadVentas = await context.Ventas.CountAsync();

        List<StockDisponibleRow> stock = await inventarioStock.GetStockDisponibleAsync(null, null);
        ViewBag.AlertasStock = stock
            .Where(x => x.AlertaStockMinimo)
            .OrderBy(x => x.CantidadDisponible)
            .ToList();
        return View();
    }
}
