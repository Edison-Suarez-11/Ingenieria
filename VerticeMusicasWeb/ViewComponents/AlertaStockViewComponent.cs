using Microsoft.AspNetCore.Mvc;
using VerticeMusicasWeb.Models;
using VerticeMusicasWeb.Services;

namespace VerticeMusicasWeb.ViewComponents;

public class AlertaStockViewComponent(InventarioStockService inventarioStock) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<StockDisponibleRow> stock = await inventarioStock.GetStockDisponibleAsync(null, null);
        List<StockDisponibleRow> alertas = stock
            .Where(x => x.AlertaStockMinimo)
            .OrderBy(x => x.CantidadDisponible)
            .ToList();

        return View(alertas);
    }
}
