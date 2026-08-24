using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Purchasing;

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> CreateAsync(string name, string? phone, string? email, string? address, string? notes);
    Task<Supplier> UpdateAsync(int id, string name, string? phone, string? email, string? address, string? notes);
    Task SetStatusAsync(int id, EntityStatus status);
    Task<decimal> GetOutstandingBalanceAsync(int supplierId);
}
