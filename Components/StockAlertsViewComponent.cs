using Microsoft.AspNetCore.Mvc;
using POS.Web.Services.Catalog;
using POS.Web.ViewModels;

namespace POS.Web.Components;

public class StockAlertsViewComponent(IProductService productService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var lowStock = await productService.GetLowStockAsync();

        var vm = new StockAlertsViewModel
        {
            LowStockCount = lowStock.Count(p => p.StockQuantity > 0),
            OutOfStockCount = lowStock.Count(p => p.StockQuantity <= 0),
            Items = lowStock
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .Take(8)
                .Select(p => new StockAlertNotification(p.Id, p.Name, p.StockQuantity, p.MinimumStockLevel))
                .ToList(),
        };

        return View(vm);
    }
}
