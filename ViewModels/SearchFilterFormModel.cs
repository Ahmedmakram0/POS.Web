namespace POS.Web.ViewModels;

// Renders the "search box + include-inactive checkbox + submit" GET filter form used
// identically by Suppliers/Index and Customers/Index.
public record SearchFilterFormModel(string? Search, bool IncludeInactive, string SearchPlaceholder);
