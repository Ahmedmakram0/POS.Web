namespace POS.Web.Models.Entities;

public class PurchaseItem
{
    public int Id { get; set; }

    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitCostPrice { get; set; }
    public decimal LineTotal { get; set; }
}
