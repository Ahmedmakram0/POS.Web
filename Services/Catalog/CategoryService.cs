using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Services.Common;

namespace POS.Web.Services.Catalog;

public class CategoryService(ApplicationDbContext db) : NamedEntityServiceBase<Category>(db), ICategoryService
{
    protected override DbSet<Category> Set => Db.Categories;

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
