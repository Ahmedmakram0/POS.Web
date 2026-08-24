using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.Web.Models.Entities;
using POS.Web.Models.Identity;

namespace POS.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerCreditTransaction> CustomerCreditTransactions => Set<CustomerCreditTransaction>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DiscountApproval> DiscountApprovals => Set<DiscountApproval>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();

    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<BusinessDay> BusinessDays => Set<BusinessDay>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<ReceiptSettings> ReceiptSettings => Set<ReceiptSettings>();
    public DbSet<DiscountSettings> DiscountSettings => Set<DiscountSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Barcode).IsUnique();
            e.HasIndex(p => p.SKU);
            e.HasIndex(p => p.Name);
            e.Property(p => p.CostPrice).HasPrecision(18, 2);
            e.Property(p => p.SellingPrice).HasPrecision(18, 2);
            e.Property(p => p.MinimumSellingPrice).HasPrecision(18, 2);
            e.HasOne(p => p.Category).WithMany(c => c.Products).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Supplier).WithMany(s => s.Products).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Store).WithMany(s => s.Products).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductPriceHistory>(e =>
        {
            e.Property(p => p.CostPrice).HasPrecision(18, 2);
            e.Property(p => p.SellingPrice).HasPrecision(18, 2);
            e.Property(p => p.MinimumSellingPrice).HasPrecision(18, 2);
            e.HasOne(p => p.Product).WithMany(p => p.PriceHistory).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockMovement>(e =>
        {
            e.HasOne(s => s.Product).WithMany(p => p.StockMovements).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Sale>(e =>
        {
            e.HasIndex(s => s.InvoiceNumber).IsUnique();
            e.Property(s => s.SubtotalBeforeDiscount).HasPrecision(18, 2);
            e.Property(s => s.TotalDiscount).HasPrecision(18, 2);
            e.Property(s => s.Total).HasPrecision(18, 2);
            e.Property(s => s.AmountPaid).HasPrecision(18, 2);
            e.Property(s => s.ChangeGiven).HasPrecision(18, 2);
            e.Property(s => s.CreditAmount).HasPrecision(18, 2);
            e.HasOne(s => s.Customer).WithMany(c => c.Sales).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleItem>(e =>
        {
            e.Property(s => s.OriginalUnitPrice).HasPrecision(18, 2);
            e.Property(s => s.DiscountValue).HasPrecision(18, 2);
            e.Property(s => s.DiscountAmountPerUnit).HasPrecision(18, 2);
            e.Property(s => s.FinalUnitPrice).HasPrecision(18, 2);
            e.Property(s => s.MinimumSellingPriceSnapshot).HasPrecision(18, 2);
            e.Property(s => s.LineTotal).HasPrecision(18, 2);
            e.HasOne(s => s.Sale).WithMany(s => s.Items).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.HasOne(p => p.Sale).WithMany(s => s.Payments).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DiscountApproval>(e =>
        {
            e.Property(d => d.OriginalUnitPrice).HasPrecision(18, 2);
            e.Property(d => d.MinimumSellingPrice).HasPrecision(18, 2);
            e.Property(d => d.RequestedFinalUnitPrice).HasPrecision(18, 2);
            e.Property(d => d.ApprovedFinalUnitPrice).HasPrecision(18, 2);
            e.HasOne(d => d.SaleItem).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Return>(e =>
        {
            e.Property(r => r.TotalRefundAmount).HasPrecision(18, 2);
            e.HasOne(r => r.Sale).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ReturnItem>(e =>
        {
            e.Property(r => r.RefundUnitPrice).HasPrecision(18, 2);
            e.Property(r => r.RefundAmount).HasPrecision(18, 2);
            e.HasOne(r => r.Return).WithMany(r => r.Items).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.SaleItem).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.Phone);
        });

        builder.Entity<CustomerCreditTransaction>(e =>
        {
            e.Property(c => c.Amount).HasPrecision(18, 2);
            e.Property(c => c.BalanceAfter).HasPrecision(18, 2);
            e.HasOne(c => c.Customer).WithMany(c => c.CreditTransactions).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Sale).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseInvoice>(e =>
        {
            e.HasIndex(p => p.InvoiceNumber).IsUnique();
            e.Property(p => p.Total).HasPrecision(18, 2);
            e.Property(p => p.AmountPaid).HasPrecision(18, 2);
            e.Property(p => p.OutstandingAmount).HasPrecision(18, 2);
            e.HasOne(p => p.Supplier).WithMany(s => s.PurchaseInvoices).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseItem>(e =>
        {
            e.Property(p => p.UnitCostPrice).HasPrecision(18, 2);
            e.Property(p => p.LineTotal).HasPrecision(18, 2);
            e.HasOne(p => p.PurchaseInvoice).WithMany(p => p.Items).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Product).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierPayment>(e =>
        {
            e.Property(s => s.Amount).HasPrecision(18, 2);
            e.HasOne(s => s.Supplier).WithMany(s => s.Payments).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.PurchaseInvoice).WithMany(p => p.Payments).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FinancialAccount>(e =>
        {
            e.Property(f => f.Balance).HasPrecision(18, 2);
            e.HasIndex(f => f.Type).IsUnique();
        });

        builder.Entity<FinancialTransaction>(e =>
        {
            e.Property(f => f.Amount).HasPrecision(18, 2);
            e.HasOne(f => f.FinancialAccount).WithMany(a => a.Transactions).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.MaxDiscountPercent).HasPrecision(5, 2);
        });

        builder.Entity<DiscountSettings>(e =>
        {
            e.Property(d => d.DefaultCashierMaxDiscountPercent).HasPrecision(5, 2);
        });

        builder.Entity<BusinessDay>(e =>
        {
            e.HasIndex(b => b.Date).IsUnique();
            e.Property(b => b.OpeningSafeBalance).HasPrecision(18, 2);
            e.Property(b => b.ExpectedSafeBalance).HasPrecision(18, 2);
            e.Property(b => b.ActualSafeBalance).HasPrecision(18, 2);
            e.Property(b => b.Difference).HasPrecision(18, 2);
        });
    }
}
