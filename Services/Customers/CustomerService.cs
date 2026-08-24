using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;
using POS.Web.Services.Financial;

namespace POS.Web.Services.Customers;

public class CustomerService(ApplicationDbContext db, IFinancialAccountService financial) : ICustomerService
{
    public async Task<List<Customer>> GetAllAsync(bool includeInactive = false, string? search = null)
    {
        var query = db.Customers.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.Status == EntityStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term) || (c.Phone != null && c.Phone.Contains(term)));
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public Task<Customer?> GetByIdAsync(int id) =>
        db.Customers.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Customer> CreateAsync(string name, string? phone, string? address, string? notes)
    {
        var customer = new Customer { Name = name, Phone = phone, Address = address, Notes = notes };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(int id, string name, string? phone, string? address, string? notes)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        customer.Name = name;
        customer.Phone = phone;
        customer.Address = address;
        customer.Notes = notes;
        await db.SaveChangesAsync();
        return customer;
    }

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        customer.Status = status;
        await db.SaveChangesAsync();
    }

    public async Task<decimal> GetBalanceAsync(int customerId)
    {
        var last = await db.CustomerCreditTransactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        return last?.BalanceAfter ?? 0m;
    }

    public Task<List<CustomerCreditTransaction>> GetCreditHistoryAsync(int customerId) =>
        db.CustomerCreditTransactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<CustomerCreditTransaction> RecordPaymentAsync(int customerId, decimal amount, PaymentMethod method, string recordedByUserId)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");
        }
        if (method == PaymentMethod.Credit)
        {
            throw new ArgumentException("Credit is not a valid settlement method for a credit payment.", nameof(method));
        }
        if (!await db.Customers.AnyAsync(c => c.Id == customerId))
        {
            throw new KeyNotFoundException($"Customer {customerId} not found.");
        }

        var currentBalance = await GetBalanceAsync(customerId);

        using var dbTransaction = await db.Database.BeginTransactionAsync();

        var creditTransaction = new CustomerCreditTransaction
        {
            CustomerId = customerId,
            Type = CustomerCreditTransactionType.Payment,
            Amount = amount,
            BalanceAfter = currentBalance - amount,
            PaymentMethod = method,
            RecordedByUserId = recordedByUserId,
        };
        db.CustomerCreditTransactions.Add(creditTransaction);
        await db.SaveChangesAsync();

        var reference = $"CustomerPayment#{creditTransaction.Id}";
        var transactionType = method == PaymentMethod.Cash
            ? FinancialTransactionType.CashCreditPayment
            : FinancialTransactionType.DigitalCreditPayment;

        await financial.PostAsync(method.ToAccountType(), transactionType, TransactionDirection.In, amount, recordedByUserId, reference);
        await financial.PostAsync(FinancialAccountType.CustomerReceivables, transactionType, TransactionDirection.Out, amount, recordedByUserId, reference);

        await dbTransaction.CommitAsync();
        return creditTransaction;
    }
}
