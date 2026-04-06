using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticeMusicasWeb.Data;

namespace VerticeMusicasWeb.Controllers;

public class DashboardController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.CantidadCategorias = await context.Categorias.CountAsync();
        ViewBag.CantidadProductos = await context.Productos.CountAsync();
        ViewBag.CantidadMovimientos = await context.MovimientosStock.CountAsync();
        return View();
    }
}
