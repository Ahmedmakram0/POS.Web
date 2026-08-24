using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class FinancialAccount
{
    public int Id { get; set; }
    public FinancialAccountType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    // Balance is derived/cached from FinancialTransactions; never edited directly.
    public decimal Balance { get; set; }

    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}
