using System.ComponentModel.DataAnnotations;
using POS.Web.Models.Enums;

namespace POS.Web.ViewModels;

public class SafeTransactionFormViewModel
{
    [Display(Name = "الحساب")]
    public FinancialAccountType AccountType { get; set; } = FinancialAccountType.CashSafe;

    [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من صفر")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "الوصف")]
    public string? Description { get; set; }
}
