namespace POS.Web.Authorization;

/// <summary>
/// Catalog of controllers a Cashier's access can be restricted to. Key equals the controller name,
/// so it can be matched directly against RouteData without a lookup table.
/// </summary>
public static class PageAccess
{
    public static readonly (string Key, string Label)[] Pages =
    [
        ("Pos", "نقطة البيع"),
        ("Sales", "المبيعات"),
        ("Products", "المنتجات"),
        ("Categories", "الفئات"),
        ("Stores", "الفروع"),
        ("Customers", "العملاء"),
        ("Suppliers", "الموردين"),
        ("Purchases", "المشتريات"),
        ("Safe", "الخزينة"),
    ];

    public static readonly HashSet<string> RestrictableControllers =
        new(Pages.Select(p => p.Key), StringComparer.OrdinalIgnoreCase);

    public static List<string> Parse(string? allowedPages) =>
        string.IsNullOrWhiteSpace(allowedPages)
            ? []
            : allowedPages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    public static string Serialize(IEnumerable<string>? keys) =>
        keys is null ? string.Empty : string.Join(',', keys.Where(k => RestrictableControllers.Contains(k)));
}
