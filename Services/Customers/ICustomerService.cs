using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Customers;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(string name, string? phone, string? address, string? notes);
    Task<Customer> UpdateAsync(int id, string name, string? phone, string? address, string? notes);
    Task SetStatusAsync(int id, EntityStatus status);

    Task<decimal> GetBalanceAsync(int customerId);
    Task<List<CustomerCreditTransaction>> GetCreditHistoryAsync(int customerId);

    /// <summary>Records a payment against a customer's credit balance and posts it against the cash/digital account.</summary>
    Task<CustomerCreditTransaction> RecordPaymentAsync(int customerId, decimal amount, PaymentMethod method, string recordedByUserId);
}
