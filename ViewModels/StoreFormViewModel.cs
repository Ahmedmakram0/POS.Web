using System.ComponentModel.DataAnnotations;

namespace POS.Web.ViewModels;

public class StoreFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [Display(Name = "اسم الفرع")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "الهاتف")]
    public string? Phone { get; set; }
}
