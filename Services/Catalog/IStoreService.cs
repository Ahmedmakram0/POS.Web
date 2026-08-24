using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public record CreateStoreRequest(string Name, string? Address, string? Phone);
public record UpdateStoreRequest(string Name, string? Address, string? Phone);
public record StoreListItemDto(int Id, string Name, string? Address, string? Phone, EntityStatus Status, int ProductCount);

public interface IStoreService
{
    Task<List<Store>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<List<StoreListItemDto>> GetAllForListAsync(bool includeInactive = false, string? search = null);
    Task<Store?> GetByIdAsync(int id);
    Task<Store> CreateAsync(CreateStoreRequest request);
    Task<Store> UpdateAsync(int id, UpdateStoreRequest request);
    Task SetStatusAsync(int id, EntityStatus status);
}
