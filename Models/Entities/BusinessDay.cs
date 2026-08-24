using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class BusinessDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public string OpenedByUserId { get; set; } = string.Empty;
    public decimal OpeningSafeBalance { get; set; }

    public DateTime? ClosedAt { get; set; }
    public string? ClosedByUserId { get; set; }
    public decimal? ExpectedSafeBalance { get; set; }
    public decimal? ActualSafeBalance { get; set; }
    public decimal? Difference { get; set; }
    public string? DifferenceNote { get; set; }

    public BusinessDayStatus Status { get; set; } = BusinessDayStatus.Open;
}
