namespace POS.Web.Models.Enums;

public enum FinancialTransactionType
{
    CashSale,
    DigitalSale,
    CreditSale,
    CashCreditPayment,
    DigitalCreditPayment,
    SupplierPayment,
    SupplierInvoiceCredit,
    CustomerRefund,
    Expense,
    ManagerWithdrawal,
    ManualDeposit,
    CashAdjustment,
    SafeOpening
}
