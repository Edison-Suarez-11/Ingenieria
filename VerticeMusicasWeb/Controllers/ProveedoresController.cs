using Microsoft.AspNetCore.Mvc;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.Controllers;

public class ProveedoresController(ProveedorService proveedorService) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        List<Proveedor> proveedores = await proveedorService.ListarAsync(q);
        ViewBag.Term = q;
        return View(proveedores);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(string? q)
    {
        List<Proveedor> proveedores = await proveedorService.ListarAsync(q);
        return Json(proveedores);
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(int id)
    {
        Proveedor? proveedor = await proveedorService.ObtenerPorIdAsync(id);
        if (proveedor is null)
        {
            return NotFound(new { exito = false, mensaje = "Proveedor no encontrado." });
        }

        return Json(proveedor);
    }

    public IActionResult Create()
    {
        return View(new Proveedor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Proveedor proveedor)
    {
        ProveedorOperacionResultado resultado = await proveedorService.CrearAsync(proveedor);

        if (EsSolicitudAjax())
        {
            return Json(resultado);
        }

        if (!resultado.Exito)
        {
            foreach (KeyValuePair<string, string[]> error in resultado.Errores)
            {
                foreach (string mensaje in error.Value)
                {
                    ModelState.AddModelError(error.Key, mensaje);
                }
            }

            if (resultado.Errores.Count == 0)
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return View(proveedor);
        }

        TempData["Success"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        Proveedor? proveedor = await proveedorService.ObtenerPorIdAsync(id);
        if (proveedor is null)
        {
            TempData["Error"] = "Proveedor no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Proveedor proveedor)
    {
        if (id != proveedor.IdProveedor)
        {
            if (EsSolicitudAjax())
            {
                return BadRequest(new ProveedorOperacionResultado
                {
                    Exito = false,
                    Mensaje = "Proveedor invalido."
                });
            }

            TempData["Error"] = "Proveedor invalido.";
            return RedirectToAction(nameof(Index));
        }

        ProveedorOperacionResultado resultado = await proveedorService.ActualizarAsync(proveedor);

        if (EsSolicitudAjax())
        {
            return Json(resultado);
        }

        if (!resultado.Exito)
        {
            foreach (KeyValuePair<string, string[]> error in resultado.Errores)
            {
                foreach (string mensaje in error.Value)
                {
                    ModelState.AddModelError(error.Key, mensaje);
                }
            }

            if (resultado.Errores.Count == 0)
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return View(proveedor);
        }

        TempData["Success"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    private bool EsSolicitudAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
