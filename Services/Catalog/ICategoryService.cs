using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public record CreateCategoryRequest(string Name);
public record UpdateCategoryRequest(string Name);
public record CategoryListItemDto(int Id, string Name, EntityStatus Status, int ProductCount);

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(bool includeInactive = false, string? search = null);
    Task<List<CategoryListItemDto>> GetAllForListAsync(bool includeInactive = false, string? search = null);
    Task<Category?> GetByIdAsync(int id);
    Task<Category> CreateAsync(CreateCategoryRequest request);
    Task<Category> UpdateAsync(int id, UpdateCategoryRequest request);
    Task SetStatusAsync(int id, EntityStatus status);
}
