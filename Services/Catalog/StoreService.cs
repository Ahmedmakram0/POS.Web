using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public class StoreService(ApplicationDbContext db) : IStoreService
{
    public async Task<List<Store>> GetAllAsync(bool includeInactive = false)
    {
        var query = db.Stores.Include(s => s.Products).AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(s => s.Status == EntityStatus.Active);
        }

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public Task<Store?> GetByIdAsync(int id) =>
        db.Stores.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Store> CreateAsync(string name, string? address, string? phone)
    {
        var store = new Store { Name = name, Address = address, Phone = phone };
        db.Stores.Add(store);
        await db.SaveChangesAsync();
        return store;
    }

    public async Task<Store> UpdateAsync(int id, string name, string? address, string? phone)
    {
        var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Store {id} not found.");

        store.Name = name;
        store.Address = address;
        store.Phone = phone;
        await db.SaveChangesAsync();
        return store;
    }

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Store {id} not found.");

        store.Status = status;
        await db.SaveChangesAsync();
    }
}
