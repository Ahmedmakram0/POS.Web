using POS.Web.Models.Enums;

namespace POS.Web.ViewModels;

// Renders the ToggleStatus POST form used identically across Products/Categories/Stores
// index rows and the Suppliers/Customers details pages (all post to the current
// controller's ToggleStatus action with the same currentStatus hidden field).
public record ToggleStatusButtonModel(int Id, EntityStatus Status, string ButtonClass = "btn btn-sm btn-outline-secondary");
