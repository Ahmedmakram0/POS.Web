using System.ComponentModel.DataAnnotations;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الباركود مطلوب")]
    [Display(Name = "الباركود")]
    public string Barcode { get; set; } = string.Empty;

    [Display(Name = "رمز الصنف (SKU)")]
    public string? SKU { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الفئة مطلوبة")]
    [Display(Name = "الفئة")]
    public int CategoryId { get; set; }

    [Display(Name = "المورد")]
    public int? SupplierId { get; set; }

    [Display(Name = "الفرع")]
    public int? StoreId { get; set; }

    [Display(Name = "الموقع داخل الفرع")]
    public string? Location { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "يجب أن يكون سعر التكلفة صفر أو أكثر")]
    [Display(Name = "سعر التكلفة")]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "يجب أن يكون سعر البيع صفر أو أكثر")]
    [Display(Name = "سعر البيع")]
    public decimal SellingPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "يجب أن يكون الحد الأدنى لسعر البيع صفر أو أكثر")]
    [Display(Name = "الحد الأدنى لسعر البيع")]
    public decimal MinimumSellingPrice { get; set; }

    [Display(Name = "الرصيد الافتتاحي")]
    public int InitialStockQuantity { get; set; }

    [Display(Name = "الحد الأدنى للمخزون")]
    public int MinimumStockLevel { get; set; }

    [Display(Name = "صورة المنتج")]
    public IFormFile? ImageFile { get; set; }

    public string? ImageUrl { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<Supplier> Suppliers { get; set; } = new();
    public List<Store> Stores { get; set; } = new();
}

public class StockAdjustmentFormViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "نوع الحركة")]
    public StockMovementType Type { get; set; } = StockMovementType.ManualAdjustment;

    [Display(Name = "الكمية (موجب للزيادة، سالب للنقصان)")]
    public int QuantityChange { get; set; }

    [Display(Name = "السبب")]
    public string? Reason { get; set; }
}
