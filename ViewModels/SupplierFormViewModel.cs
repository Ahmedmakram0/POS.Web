using System.ComponentModel.DataAnnotations;

namespace POS.Web.ViewModels;

public class SupplierFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الهاتف")]
    public string? Phone { get; set; }

    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
