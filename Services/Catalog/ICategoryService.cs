using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(bool includeInactive = false);
    Task<Category?> GetByIdAsync(int id);
    Task<Category> CreateAsync(string name);
    Task<Category> UpdateAsync(int id, string name);
    Task SetStatusAsync(int id, EntityStatus status);
}
