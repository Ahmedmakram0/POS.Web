using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Services.Common;

namespace POS.Web.Services.Purchasing;

public class SupplierService(ApplicationDbContext db) : NamedEntityServiceBase<Supplier>(db), ISupplierService
{
    protected override DbSet<Supplier> Set => Db.Suppliers;

    protected override IQueryable<Supplier> ApplySearch(IQueryable<Supplier> query, string term) =>
        query.Where(s => s.Name.Contains(term) || (s.Phone != null && s.Phone.Contains(term)));

    public async Task<Supplier> CreateAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name, Phone = request.Phone, Email = request.Email,
            Address = request.Address, Notes = request.Notes,
        };
        Db.Suppliers.Add(supplier);
        await Db.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await GetRequiredAsync(id);
        supplier.Name = request.Name;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.Address = request.Address;
        supplier.Notes = request.Notes;
        await Db.SaveChangesAsync();
        return supplier;
    }

    public Task<decimal> GetOutstandingBalanceAsync(int supplierId) =>
        Db.PurchaseInvoices.Where(p => p.SupplierId == supplierId).SumAsync(p => p.OutstandingAmount);
}
