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

    /// <summary>Posts an Out transaction after checking the account has sufficient balance.</summary>
    /// <exception cref="InvalidOperationException">The account balance is lower than <paramref name="amount"/>.</exception>
    Task<FinancialTransaction> WithdrawAsync(
        FinancialAccountType accountType,
        FinancialTransactionType type,
        decimal amount,
        string createdByUserId,
        string? description = null);
}
