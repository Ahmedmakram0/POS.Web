using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Authorization;
using POS.Web.Services.Catalog;
using POS.Web.Services.Customers;
using POS.Web.Services.Sales;

namespace POS.Web.Controllers;

[Authorize]
public class PosController(ISaleService saleService, IProductService productService, ICustomerService customerService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Customers"] = await customerService.GetAllAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SearchProducts(string? q)
    {
        var products = await productService.GetAllAsync(new ProductFilter(q));
        var result = products.Take(25).Select(p => new
        {
            id = p.Id,
            barcode = p.Barcode,
            name = p.Name,
            price = p.SellingPrice,
            minPrice = p.MinimumSellingPrice,
            stock = p.StockQuantity,
            store = p.Store?.Name,
            location = p.Location,
            imageUrl = p.ImageUrl,
        });
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> LookupByBarcode(string barcode)
    {
        var product = await productService.GetByBarcodeAsync(barcode);
        if (product is null)
        {
            return NotFound();
        }

        return Json(new
        {
            id = product.Id,
            barcode = product.Barcode,
            name = product.Name,
            price = product.SellingPrice,
            minPrice = product.MinimumSellingPrice,
            stock = product.StockQuantity,
            store = product.Store?.Name,
            location = product.Location,
            imageUrl = product.ImageUrl,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([FromBody] CreateSaleRequest request)
    {
        if (request is null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "لا يوجد أصناف في الفاتورة." });
        }

        try
        {
            var sale = await saleService.CreateAsync(request, this.GetCurrentUserId());
            return Json(new { saleId = sale.Id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
