using System.ComponentModel.DataAnnotations;
using POS.Web.Models.Enums;

namespace POS.Web.ViewModels;

public class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الهاتف")]
    public string? Phone { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}

public class CustomerPaymentFormViewModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من صفر")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
}
