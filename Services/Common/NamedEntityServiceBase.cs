using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Common;

// Shared GetAll/GetById/SetStatus behavior for the simple named-entity CRUD services
// (Category, Store, Supplier, Customer). Create/Update stay on each concrete service since
// their field lists differ; GetRequiredAsync is exposed so those methods share the same
// "not found" lookup instead of repeating it.
public abstract class NamedEntityServiceBase<TEntity>(ApplicationDbContext db) where TEntity : class, INamedEntity
{
    protected ApplicationDbContext Db { get; } = db;

    protected abstract DbSet<TEntity> Set { get; }

    // Override to add .Include(...) for list queries; defaults to the bare DbSet.
    protected virtual IQueryable<TEntity> ListQuery => Set;

    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string term) =>
        query.Where(e => e.Name.Contains(term));

    public async Task<List<TEntity>> GetAllAsync(bool includeInactive = false, string? search = null)
    {
        var query = ListQuery;

        if (!includeInactive)
        {
            query = query.Where(e => e.Status == EntityStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = ApplySearch(query, search.Trim());
        }

        return await query.OrderBy(e => e.Name).ToListAsync();
    }

    public Task<TEntity?> GetByIdAsync(int id) => Set.FirstOrDefaultAsync(e => e.Id == id);

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var entity = await GetRequiredAsync(id);
        entity.Status = status;
        await Db.SaveChangesAsync();
    }

    protected async Task<TEntity> GetRequiredAsync(int id) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} {id} not found.");
}
