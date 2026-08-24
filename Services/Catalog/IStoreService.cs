using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public interface IStoreService
{
    Task<List<Store>> GetAllAsync(bool includeInactive = false);
    Task<Store?> GetByIdAsync(int id);
    Task<Store> CreateAsync(string name, string? address, string? phone);
    Task<Store> UpdateAsync(int id, string name, string? address, string? phone);
    Task SetStatusAsync(int id, EntityStatus status);
}
