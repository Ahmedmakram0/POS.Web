using System.ComponentModel.DataAnnotations;

namespace POS.Web.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    [Display(Name = "اسم الفئة")]
    public string Name { get; set; } = string.Empty;
}
