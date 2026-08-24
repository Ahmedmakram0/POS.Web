using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class Store : INamedEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
