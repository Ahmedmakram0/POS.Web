using Microsoft.AspNetCore.Identity;

namespace POS.Web.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }

    // Discount authorization: max discount, as a percentage of selling price, this user may apply without approval.
    public decimal MaxDiscountPercent { get; set; }

    // If true, user may apply discounts down to the product's minimum selling price without approval.
    public bool CanDiscountToMinimumPrice { get; set; }

    // If true, user may override the minimum selling price entirely (sell below it).
    public bool CanOverrideMinimumPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Comma-separated POS.Web.Authorization.PageAccess keys this user (a Cashier) may open.
    // Ignored for SuperAdmin/Admin, who always have full access.
    public string? AllowedPages { get; set; }
}
