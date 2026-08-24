using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Purchasing;

public class SupplierService(ApplicationDbContext db) : ISupplierService
{
    public async Task<List<Supplier>> GetAllAsync(bool includeInactive = false, string? search = null)
    {
        var query = db.Suppliers.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(s => s.Status == EntityStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Name.Contains(term) || (s.Phone != null && s.Phone.Contains(term)));
        }

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public Task<Supplier?> GetByIdAsync(int id) =>
        db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Supplier> CreateAsync(string name, string? phone, string? email, string? address, string? notes)
    {
        var supplier = new Supplier { Name = name, Phone = phone, Email = email, Address = address, Notes = notes };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateAsync(int id, string name, string? phone, string? email, string? address, string? notes)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Supplier {id} not found.");

        supplier.Name = name;
        supplier.Phone = phone;
        supplier.Email = email;
        supplier.Address = address;
        supplier.Notes = notes;
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Supplier {id} not found.");

        supplier.Status = status;
        await db.SaveChangesAsync();
    }

    public Task<decimal> GetOutstandingBalanceAsync(int supplierId) =>
        db.PurchaseInvoices.Where(p => p.SupplierId == supplierId).SumAsync(p => p.OutstandingAmount);
}
