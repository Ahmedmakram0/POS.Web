using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Services.Common;

namespace POS.Web.Services.Catalog;

public class StoreService(ApplicationDbContext db) : NamedEntityServiceBase<Store>(db), IStoreService
{
    protected override DbSet<Store> Set => Db.Stores;

    // Views/Stores/Index.cshtml shows a per-store product count.
    protected override IQueryable<Store> ListQuery => Db.Stores.Include(s => s.Products);

    public async Task<Store> CreateAsync(CreateStoreRequest request)
    {
        var store = new Store { Name = request.Name, Address = request.Address, Phone = request.Phone };
        Db.Stores.Add(store);
        await Db.SaveChangesAsync();
        return store;
    }

    public async Task<Store> UpdateAsync(int id, UpdateStoreRequest request)
    {
        var store = await GetRequiredAsync(id);
        store.Name = request.Name;
        store.Address = request.Address;
        store.Phone = request.Phone;
        await Db.SaveChangesAsync();
        return store;
    }
}
