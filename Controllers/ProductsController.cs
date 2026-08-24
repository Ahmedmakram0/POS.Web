using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Authorization;
using POS.Web.Models.Enums;
using POS.Web.Services.Catalog;
using POS.Web.Services.Media;
using POS.Web.Services.Purchasing;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class ProductsController(
    IProductService productService, ICategoryService categoryService, IInventoryService inventoryService,
    ISupplierService supplierService, IStoreService storeService, IProductImageService productImageService)
    : Controller
{

    public async Task<IActionResult> Index(string? search, int? categoryId, int? storeId, bool includeInactive = false, bool onlyLowStock = false)
    {
        var filter = new ProductFilter(search, categoryId, includeInactive, onlyLowStock, storeId);
        ViewData["Search"] = search;
        ViewData["CategoryId"] = categoryId;
        ViewData["StoreId"] = storeId;
        ViewData["IncludeInactive"] = includeInactive;
        ViewData["OnlyLowStock"] = onlyLowStock;
        ViewData["Categories"] = await categoryService.GetAllAsync(includeInactive: true);
        ViewData["Stores"] = await storeService.GetAllAsync(includeInactive: true);
        return View(await productService.GetAllAsync(filter));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Movements"] = await inventoryService.GetMovementsForProductAsync(id);
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel
        {
            Categories = await categoryService.GetAllAsync(),
            Suppliers = await GetSuppliersAsync(),
            Stores = await GetStoresAsync(),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await categoryService.GetAllAsync();
            model.Suppliers = await GetSuppliersAsync();
            model.Stores = await GetStoresAsync();
            return View(model);
        }

        try
        {
            string? imageUrl = null;
            string? imagePublicId = null;
            if (model.ImageFile is { Length: > 0 } imageFile)
            {
                await using var stream = imageFile.OpenReadStream();
                var uploaded = await productImageService.UploadAsync(stream, imageFile.FileName);
                imageUrl = uploaded.Url;
                imagePublicId = uploaded.PublicId;
            }

            var request = new ProductCreateRequest(
                model.Barcode, model.SKU, model.Name, model.CategoryId, model.SupplierId,
                model.CostPrice, model.SellingPrice, model.MinimumSellingPrice,
                model.InitialStockQuantity, model.MinimumStockLevel, model.StoreId, model.Location,
                imageUrl, imagePublicId);
            var product = await productService.CreateAsync(request, this.GetCurrentUserId());
            TempData["Success"] = "تم إضافة المنتج بنجاح. يمكنك الآن طباعة ملصق الباركود.";
            return RedirectToAction(nameof(Details), new { id = product.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categories = await categoryService.GetAllAsync();
            model.Suppliers = await GetSuppliersAsync();
            model.Stores = await GetStoresAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Barcode = product.Barcode,
            SKU = product.SKU,
            Name = product.Name,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            StoreId = product.StoreId,
            Location = product.Location,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            MinimumSellingPrice = product.MinimumSellingPrice,
            MinimumStockLevel = product.MinimumStockLevel,
            ImageUrl = product.ImageUrl,
            Categories = await categoryService.GetAllAsync(),
            Suppliers = await GetSuppliersAsync(),
            Stores = await GetStoresAsync(),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await categoryService.GetAllAsync();
            model.Suppliers = await GetSuppliersAsync();
            model.Stores = await GetStoresAsync();
            return View(model);
        }

        try
        {
            await productService.UpdateDetailsAsync(
                id, model.Barcode, model.SKU, model.Name, model.CategoryId, model.SupplierId, model.MinimumStockLevel,
                model.StoreId, model.Location);
            await productService.UpdatePricingAsync(id, model.CostPrice, model.SellingPrice, model.MinimumSellingPrice, this.GetCurrentUserId(), "تعديل يدوي");

            if (model.ImageFile is { Length: > 0 } imageFile)
            {
                var existing = await productService.GetByIdAsync(id);
                await using var stream = imageFile.OpenReadStream();
                var uploaded = await productImageService.UploadAsync(stream, imageFile.FileName);
                await productService.UpdateImageAsync(id, uploaded.Url, uploaded.PublicId);

                if (!string.IsNullOrEmpty(existing?.ImagePublicId))
                {
                    await productImageService.DeleteAsync(existing.ImagePublicId);
                }
            }

            TempData["Success"] = "تم تحديث المنتج بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categories = await categoryService.GetAllAsync();
            model.Suppliers = await GetSuppliersAsync();
            model.Stores = await GetStoresAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateBarcode()
    {
        var random = new Random();
        string barcode;
        do
        {
            barcode = GenerateEan13(random);
        }
        while (await productService.GetByBarcodeAsync(barcode) is not null);

        return Json(new { barcode });
    }

    // Lets other screens (e.g. the purchase invoice form) register a brand-new product inline,
    // without a full round trip through the Products/Create page. Stock always starts at zero here:
    // whatever brought the caller here (a purchase invoice, a stock take, ...) is responsible for it.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Barcode) || string.IsNullOrWhiteSpace(request.Name) || request.CategoryId <= 0)
        {
            return BadRequest(new { message = "الباركود والاسم والفئة مطلوبة." });
        }

        try
        {
            var product = await productService.CreateAsync(
                new ProductCreateRequest(
                    request.Barcode.Trim(), request.SKU, request.Name.Trim(), request.CategoryId, request.SupplierId,
                    request.CostPrice, request.SellingPrice, request.MinimumSellingPrice, 0, request.MinimumStockLevel,
                    request.StoreId, request.Location),
                this.GetCurrentUserId());

            return Json(new
            {
                id = product.Id, name = product.Name, barcode = product.Barcode, costPrice = product.CostPrice,
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Resolves a scanned barcode or a typed product id to a product, for item-entry rows
    // (e.g. purchase invoice lines) that accept either without a dropdown.
    [HttpGet]
    public async Task<IActionResult> LookupProduct(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return NotFound();
        }

        var product = int.TryParse(code, out var id) ? await productService.GetByIdAsync(id) : null;
        product ??= await productService.GetByBarcodeAsync(code.Trim());

        if (product is null)
        {
            return NotFound();
        }

        return Json(new
        {
            id = product.Id, name = product.Name, barcode = product.Barcode, costPrice = product.CostPrice,
        });
    }

    [HttpGet]
    public async Task<IActionResult> PrintLabels(int id, int copies = 12)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Copies"] = Math.Clamp(copies, 1, 100);
        return View(product);
    }

    // Generates an EAN-13 barcode in the 200-299 "internal use" prefix range, with a valid check digit.
    private static string GenerateEan13(Random random)
    {
        var digits = new int[13];
        digits[0] = 2;
        for (var i = 1; i < 12; i++)
        {
            digits[i] = random.Next(0, 10);
        }

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += digits[i] * (i % 2 == 0 ? 1 : 3);
        }
        digits[12] = (10 - (sum % 10)) % 10;

        return string.Concat(digits);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, EntityStatus currentStatus)
    {
        var newStatus = currentStatus == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        await productService.SetStatusAsync(id, newStatus);
        TempData["Success"] = "تم تحديث حالة المنتج.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AdjustStock(int id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(new StockAdjustmentFormViewModel { ProductId = id, ProductName = product.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(StockAdjustmentFormViewModel model)
    {
        if (!ModelState.IsValid || model.QuantityChange == 0)
        {
            if (model.QuantityChange == 0)
            {
                ModelState.AddModelError(nameof(model.QuantityChange), "يجب أن تكون الكمية مختلفة عن صفر.");
            }
            return View(model);
        }

        try
        {
            await inventoryService.AdjustStockAsync(model.ProductId, model.QuantityChange, model.Type, this.GetCurrentUserId(), reason: model.Reason);
            TempData["Success"] = "تم تعديل المخزون بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private Task<List<Models.Entities.Supplier>> GetSuppliersAsync() => supplierService.GetAllAsync();
    private Task<List<Models.Entities.Store>> GetStoresAsync() => storeService.GetAllAsync();
}

public record QuickCreateProductRequest(
    string Barcode, string? SKU, string Name, int CategoryId,
    decimal CostPrice, decimal SellingPrice, decimal MinimumSellingPrice, int MinimumStockLevel,
    int? SupplierId = null, int? StoreId = null, string? Location = null);
