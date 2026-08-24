namespace POS.Web.Models.Entities;

// Singleton row (Id = 1) holding default discount-authorization configuration.
// Per-user overrides live on ApplicationUser (MaxDiscountPercent, CanDiscountToMinimumPrice, CanOverrideMinimumPrice).
public class DiscountSettings
{
    public int Id { get; set; }

    public decimal DefaultCashierMaxDiscountPercent { get; set; } = 5m;
    public bool RequireManagerApprovalAboveLimit { get; set; } = true;
}
