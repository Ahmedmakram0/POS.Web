using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class Return
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public string ProcessedByUserId { get; set; } = string.Empty;

    public decimal TotalRefundAmount { get; set; }
    public PaymentMethod RefundMethod { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
