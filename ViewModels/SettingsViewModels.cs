namespace POS.Web.ViewModels;

public class SettingsViewModel
{
    public Models.Entities.SystemSettings System { get; set; } = new();
    public Models.Entities.ReceiptSettings Receipt { get; set; } = new();
    public Models.Entities.DiscountSettings Discount { get; set; } = new();
}
