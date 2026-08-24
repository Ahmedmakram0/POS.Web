using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Financial;

public class FinancialAccountService(ApplicationDbContext db) : IFinancialAccountService
{
    public static readonly Dictionary<FinancialAccountType, string> DefaultNames = new()
    {
        [FinancialAccountType.CashSafe] = "الخزينة النقدية",
        [FinancialAccountType.InstaPay] = "إنستاباي",
        [FinancialAccountType.VodafoneCash] = "فودافون كاش",
        [FinancialAccountType.CustomerReceivables] = "أرصدة العملاء",
        [FinancialAccountType.SupplierPayables] = "أرصدة الموردين",
    };

    public Task<List<FinancialAccount>> GetAllAccountsAsync() =>
        db.FinancialAccounts.OrderBy(a => a.Type).ToListAsync();

    public async Task<List<FinancialTransaction>> GetTransactionsAsync(FinancialAccountType? type = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var query = db.FinancialTransactions.Include(t => t.FinancialAccount).AsQueryable();

        if (type is FinancialAccountType accountType)
        {
            query = query.Where(t => t.FinancialAccount!.Type == accountType);
        }
        if (fromUtc is DateTime from)
        {
            query = query.Where(t => t.CreatedAt >= from);
        }
        if (toUtc is DateTime to)
        {
            query = query.Where(t => t.CreatedAt <= to);
        }

        return await query.OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync();
    }

    public async Task<FinancialAccount> GetOrCreateAccountAsync(FinancialAccountType type)
    {
        var account = await db.FinancialAccounts.FirstOrDefaultAsync(a => a.Type == type);
        if (account is not null)
        {
            return account;
        }

        account = new FinancialAccount { Type = type, Name = DefaultNames[type], Balance = 0 };
        db.FinancialAccounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    public async Task<decimal> GetBalanceAsync(FinancialAccountType type)
    {
        var account = await GetOrCreateAccountAsync(type);
        return account.Balance;
    }

    public async Task<FinancialTransaction> PostAsync(
        FinancialAccountType accountType,
        FinancialTransactionType type,
        TransactionDirection direction,
        decimal amount,
        string createdByUserId,
        string? reference = null,
        string? description = null)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be positive.");
        }

        var account = await GetOrCreateAccountAsync(accountType);

        account.Balance += direction == TransactionDirection.In ? amount : -amount;

        var transaction = new FinancialTransaction
        {
            FinancialAccountId = account.Id,
            Type = type,
            Direction = direction,
            Amount = amount,
            Reference = reference,
            Description = description,
            CreatedByUserId = createdByUserId,
        };

        db.FinancialTransactions.Add(transaction);
        await db.SaveChangesAsync();
        return transaction;
    }

    public async Task<FinancialTransaction> WithdrawAsync(
        FinancialAccountType accountType, FinancialTransactionType type, decimal amount, string createdByUserId, string? description = null)
    {
        var balance = await GetBalanceAsync(accountType);
        if (amount > balance)
        {
            throw new InvalidOperationException($"الرصيد الحالي ({balance:N2}) أقل من مبلغ السحب.");
        }

        return await PostAsync(accountType, type, TransactionDirection.Out, amount, createdByUserId, description: description);
    }
}
