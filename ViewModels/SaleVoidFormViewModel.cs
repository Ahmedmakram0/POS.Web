using System.ComponentModel.DataAnnotations;

namespace POS.Web.ViewModels;

public class SaleVoidFormViewModel
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
    [Display(Name = "سبب الإلغاء")]
    public string Reason { get; set; } = string.Empty;
}
