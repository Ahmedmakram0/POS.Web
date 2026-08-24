using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;
using POS.Web.Services.Common;
using POS.Web.Services.Financial;

namespace POS.Web.Services.Customers;

public class CustomerService(ApplicationDbContext db, IFinancialAccountService financial)
    : NamedEntityServiceBase<Customer>(db), ICustomerService
{
    protected override DbSet<Customer> Set => Db.Customers;

    protected override IQueryable<Customer> ApplySearch(IQueryable<Customer> query, string term) =>
        query.Where(c => c.Name.Contains(term) || (c.Phone != null && c.Phone.Contains(term)));

    public async Task<Customer> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer { Name = request.Name, Phone = request.Phone, Address = request.Address, Notes = request.Notes };
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await GetRequiredAsync(id);
        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Address = request.Address;
        customer.Notes = request.Notes;
        await Db.SaveChangesAsync();
        return customer;
    }

    public async Task<decimal> GetBalanceAsync(int customerId)
    {
        var last = await Db.CustomerCreditTransactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        return last?.BalanceAfter ?? 0m;
    }

    public Task<List<CustomerCreditTransaction>> GetCreditHistoryAsync(int customerId) =>
        Db.CustomerCreditTransactions
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
        if (!await Db.Customers.AnyAsync(c => c.Id == customerId))
        {
            throw new KeyNotFoundException($"Customer {customerId} not found.");
        }

        var currentBalance = await GetBalanceAsync(customerId);

        using var dbTransaction = await Db.Database.BeginTransactionAsync();

        var creditTransaction = new CustomerCreditTransaction
        {
            CustomerId = customerId,
            Type = CustomerCreditTransactionType.Payment,
            Amount = amount,
            BalanceAfter = currentBalance - amount,
            PaymentMethod = method,
            RecordedByUserId = recordedByUserId,
        };
        Db.CustomerCreditTransactions.Add(creditTransaction);
        await Db.SaveChangesAsync();

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
