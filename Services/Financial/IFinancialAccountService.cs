using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Financial;

public interface IFinancialAccountService
{
    Task<List<FinancialAccount>> GetAllAccountsAsync();
    Task<FinancialAccount> GetOrCreateAccountAsync(FinancialAccountType type);
    Task<decimal> GetBalanceAsync(FinancialAccountType type);

    Task<List<FinancialTransaction>> GetTransactionsAsync(FinancialAccountType? type = null, DateTime? fromUtc = null, DateTime? toUtc = null);

    Task<FinancialTransaction> PostAsync(
        FinancialAccountType accountType,
        FinancialTransactionType type,
        TransactionDirection direction,
        decimal amount,
        string createdByUserId,
        string? reference = null,
        string? description = null);
}
