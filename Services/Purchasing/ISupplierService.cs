using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Purchasing;

public record CreateSupplierRequest(string Name, string? Phone, string? Email, string? Address, string? Notes);
public record UpdateSupplierRequest(string Name, string? Phone, string? Email, string? Address, string? Notes);

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> CreateAsync(CreateSupplierRequest request);
    Task<Supplier> UpdateAsync(int id, UpdateSupplierRequest request);
    Task SetStatusAsync(int id, EntityStatus status);
    Task<decimal> GetOutstandingBalanceAsync(int supplierId);
}
