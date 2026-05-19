using Microsoft.AspNetCore.Mvc;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class InformesController(InformesService informes) : Controller
{
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        InformesViewModel vm = await informes.ObtenerInformesAsync(desde, hasta);
        return View(vm);
    }
}
