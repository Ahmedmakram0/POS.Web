using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Services.Common;

namespace POS.Web.Services.Catalog;

public class CategoryService(ApplicationDbContext db) : NamedEntityServiceBase<Category>(db), ICategoryService
{
    protected override DbSet<Category> Set => Db.Categories;

    public async Task<List<CategoryListItemDto>> GetAllForListAsync(bool includeInactive = false, string? search = null)
    {
        var query = Db.Categories.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.Status == Models.Enums.EntityStatus.Active);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListItemDto(c.Id, c.Name, c.Status, c.Products.Count))
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category { Name = request.Name };
        Db.Categories.Add(category);
        await Db.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await GetRequiredAsync(id);
        category.Name = request.Name;
        await Db.SaveChangesAsync();
        return category;
    }
}
