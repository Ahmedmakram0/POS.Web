namespace POS.Web.Models.Enums;

public enum EntityStatus
{
    Active,
    Inactive
}

public enum StockMovementType
{
    Purchase,
    Sale,
    CustomerReturn,
    SupplierReturn,
    Damage,
    ManualAdjustment,
    Correction
}

public enum PaymentMethod
{
    Cash,
    InstaPay,
    VodafoneCash,
    Credit
}

public enum DiscountType
{
    FixedAmount,
    Percentage,
    FinalUnitPrice
}

public enum SaleStatus
{
    Completed,
    Voided,
    PartiallyRefunded,
    Refunded
}

public enum PurchaseInvoiceStatus
{
    Paid,
    Unpaid,
    PartiallyPaid
}

public enum FinancialAccountType
{
    CashSafe,
    InstaPay,
    VodafoneCash,
    CustomerReceivables,
    SupplierPayables
}

public enum TransactionDirection
{
    In,
    Out
}

public enum FinancialTransactionType
{
    CashSale,
    DigitalSale,
    CreditSale,
    CashCreditPayment,
    DigitalCreditPayment,
    SupplierPayment,
    CustomerRefund,
    Expense,
    ManagerWithdrawal,
    ManualDeposit,
    CashAdjustment,
    SafeOpening
}

public enum BusinessDayStatus
{
    Open,
    Closed
}
