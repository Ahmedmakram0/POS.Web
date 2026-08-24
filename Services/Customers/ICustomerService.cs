using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Customers;

public record CreateCustomerRequest(string Name, string? Phone, string? Address, string? Notes);
public record UpdateCustomerRequest(string Name, string? Phone, string? Address, string? Notes);

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(CreateCustomerRequest request);
    Task<Customer> UpdateAsync(int id, UpdateCustomerRequest request);
    Task SetStatusAsync(int id, EntityStatus status);

    Task<decimal> GetBalanceAsync(int customerId);
    Task<List<CustomerCreditTransaction>> GetCreditHistoryAsync(int customerId);

    /// <summary>Records a payment against a customer's credit balance and posts it against the cash/digital account.</summary>
    Task<CustomerCreditTransaction> RecordPaymentAsync(int customerId, decimal amount, PaymentMethod method, string recordedByUserId);
}
