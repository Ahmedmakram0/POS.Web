using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

// Implemented by the simple "named party with a status" entities (Category, Store, Supplier,
// Customer) so NamedEntityServiceBase can provide their shared CRUD behavior once.
public interface INamedEntity
{
    int Id { get; }
    string Name { get; set; }
    EntityStatus Status { get; set; }
}
