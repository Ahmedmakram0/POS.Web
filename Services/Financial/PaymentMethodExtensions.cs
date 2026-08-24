using POS.Web.Models.Enums;

namespace POS.Web.Services.Financial;

public static class PaymentMethodExtensions
{
    public static FinancialAccountType ToAccountType(this PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => FinancialAccountType.CashSafe,
        PaymentMethod.InstaPay => FinancialAccountType.InstaPay,
        PaymentMethod.VodafoneCash => FinancialAccountType.VodafoneCash,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Payment method has no corresponding financial account."),
    };
}
