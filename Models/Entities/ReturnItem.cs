namespace POS.Web.Models.Entities;

public class ReturnItem
{
    public int Id { get; set; }

    public int ReturnId { get; set; }
    public Return? Return { get; set; }

    public int SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }

    public int Quantity { get; set; }

    // The actual final unit price paid at the time of the original sale.
    public decimal RefundUnitPrice { get; set; }
    public decimal RefundAmount { get; set; }
}
