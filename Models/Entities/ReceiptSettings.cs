namespace POS.Web.Models.Entities;

// Singleton row (Id = 1) holding thermal receipt configuration.
public class ReceiptSettings
{
    public int Id { get; set; }

    public string? LogoUrl { get; set; }
    public string ReceiptTitle { get; set; } = "فاتورة بيع";
    public string? TaxInfo { get; set; }
    public string? FooterMessage { get; set; } = "شكراً لتعاملكم معنا";

    // 58 or 80 (mm)
    public int ReceiptWidthMm { get; set; } = 80;
}
