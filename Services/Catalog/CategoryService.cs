using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public class CategoryService(ApplicationDbContext db) : ICategoryService
{
    public async Task<List<Category>> GetAllAsync(bool includeInactive = false)
    {
        var query = db.Categories.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(c => c.Status == EntityStatus.Active);
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public Task<Category?> GetByIdAsync(int id) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category> CreateAsync(string name)
    {
        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(int id, string name)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Name = name;
        await db.SaveChangesAsync();
        return category;
    }

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Status = status;
        await db.SaveChangesAsync();
    }
}
