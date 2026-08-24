using System.ComponentModel.DataAnnotations;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.ViewModels;

public class PurchaseInvoiceItemFormViewModel
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCostPrice { get; set; }
}

public class PurchaseInvoiceFormViewModel
{
    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Display(Name = "رقم فاتورة المورد")]
    public string? SupplierInvoiceReference { get; set; }

    public List<PurchaseInvoiceItemFormViewModel> Items { get; set; } = new();

    [Display(Name = "المبلغ المدفوع الآن")]
    public decimal AmountPaidNow { get; set; }

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod? PaymentMethod { get; set; }

    public List<Supplier> Suppliers { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Store> Stores { get; set; } = new();
}

public class SupplierPaymentFormViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Outstanding { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من صفر")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
}
