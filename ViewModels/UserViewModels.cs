using System.ComponentModel.DataAnnotations;

namespace POS.Web.ViewModels;

// Shared by UserCreateViewModel/UserEditViewModel so Views/Users/_PermissionsFields.cshtml
// can render the role/discount/allowed-pages block once for both Create and Edit.
public interface IUserPermissionsFormModel
{
    string Role { get; set; }
    decimal MaxDiscountPercent { get; set; }
    bool CanDiscountToMinimumPrice { get; set; }
    bool CanOverrideMinimumPrice { get; set; }
    List<string> AllowedPages { get; set; }
    List<string> AvailableRoles { get; set; }
    (string Key, string Label)[] AvailablePages { get; set; }
}

public class UserCreateViewModel : IUserPermissionsFormModel
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "الدور مطلوب")]
    [Display(Name = "الدور")]
    public string Role { get; set; } = "Cashier";

    [Range(0, 100)]
    [Display(Name = "الحد الأقصى لنسبة الخصم (%)")]
    public decimal MaxDiscountPercent { get; set; }

    [Display(Name = "يمكنه الخصم حتى الحد الأدنى لسعر البيع")]
    public bool CanDiscountToMinimumPrice { get; set; }

    [Display(Name = "يمكنه تجاوز الحد الأدنى لسعر البيع")]
    public bool CanOverrideMinimumPrice { get; set; }

    [Display(Name = "الصفحات المسموح بفتحها")]
    public List<string> AllowedPages { get; set; } = new();

    public List<string> AvailableRoles { get; set; } = new();
    public (string Key, string Label)[] AvailablePages { get; set; } = [];
}

public class UserEditViewModel : IUserPermissionsFormModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الدور مطلوب")]
    [Display(Name = "الدور")]
    public string Role { get; set; } = "Cashier";

    [Range(0, 100)]
    [Display(Name = "الحد الأقصى لنسبة الخصم (%)")]
    public decimal MaxDiscountPercent { get; set; }

    [Display(Name = "يمكنه الخصم حتى الحد الأدنى لسعر البيع")]
    public bool CanDiscountToMinimumPrice { get; set; }

    [Display(Name = "يمكنه تجاوز الحد الأدنى لسعر البيع")]
    public bool CanOverrideMinimumPrice { get; set; }

    [Display(Name = "الصفحات المسموح بفتحها")]
    public List<string> AllowedPages { get; set; } = new();

    public List<string> AvailableRoles { get; set; } = new();
    public (string Key, string Label)[] AvailablePages { get; set; } = [];
}
