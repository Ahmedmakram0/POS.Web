using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;
using POS.Web.Services.Catalog;
using POS.Web.Services.Financial;

namespace POS.Web.Services.Purchasing;

public class PurchaseInvoiceService(ApplicationDbContext db, IInventoryService inventory, IFinancialAccountService financial)
    : IPurchaseInvoiceService
{
    public async Task<List<PurchaseInvoice>> GetAllAsync(int? supplierId = null, PurchaseInvoiceStatus? status = null)
    {
        var query = db.PurchaseInvoices.Include(p => p.Supplier).AsQueryable();

        if (supplierId is int sId)
        {
            query = query.Where(p => p.SupplierId == sId);
        }

        if (status is PurchaseInvoiceStatus s)
        {
            query = query.Where(p => p.Status == s);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<List<PurchaseInvoiceListItemDto>> GetAllForListAsync(int? supplierId = null, PurchaseInvoiceStatus? status = null)
    {
        var query = db.PurchaseInvoices.AsQueryable();

        if (supplierId is int sId)
        {
            query = query.Where(p => p.SupplierId == sId);
        }
        if (status is PurchaseInvoiceStatus s)
        {
            query = query.Where(p => p.Status == s);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PurchaseInvoiceListItemDto(
                p.Id, p.InvoiceNumber, p.SupplierInvoiceReference, p.Supplier!.Name,
                p.CreatedAt, p.Total, p.AmountPaid, p.OutstandingAmount, p.Status))
            .ToListAsync();
    }

    public Task<PurchaseInvoice?> GetByIdAsync(int id) =>
        db.PurchaseInvoices
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .Include(p => p.Payments)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PurchaseInvoice> CreateAsync(CreatePurchaseInvoiceRequest request, string createdByUserId)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Purchase invoice must contain at least one item.");
        }
        if (!await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId))
        {
            throw new KeyNotFoundException($"Supplier {request.SupplierId} not found.");
        }

        var total = request.Items.Sum(i => i.Quantity * i.UnitCostPrice);
        var amountPaid = Math.Clamp(request.AmountPaidNow, 0, total);

        using var dbTransaction = await db.Database.BeginTransactionAsync();

        var today = DateTime.UtcNow.Date;
        var todaysCount = await db.PurchaseInvoices.CountAsync(p => p.CreatedAt >= today);
        var invoiceNumber = $"P-{today:yyyyMMdd}-{todaysCount + 1:0000}";

        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = invoiceNumber,
            SupplierInvoiceReference = string.IsNullOrWhiteSpace(request.SupplierInvoiceReference) ? null : request.SupplierInvoiceReference.Trim(),
            SupplierId = request.SupplierId,
            CreatedByUserId = createdByUserId,
            Total = total,
            AmountPaid = amountPaid,
            OutstandingAmount = total - amountPaid,
            Status = amountPaid <= 0 ? PurchaseInvoiceStatus.Unpaid
                : amountPaid >= total ? PurchaseInvoiceStatus.Paid
                : PurchaseInvoiceStatus.PartiallyPaid,
        };
        db.PurchaseInvoices.Add(invoice);
        await db.SaveChangesAsync();

        foreach (var item in request.Items)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId)
                ?? throw new KeyNotFoundException($"Product {item.ProductId} not found.");

            db.PurchaseItems.Add(new PurchaseItem
            {
                PurchaseInvoiceId = invoice.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitCostPrice = item.UnitCostPrice,
                LineTotal = item.Quantity * item.UnitCostPrice,
            });

            product.CostPrice = item.UnitCostPrice;
            product.UpdatedAt = DateTime.UtcNow;

            await inventory.AdjustStockAsync(
                item.ProductId, item.Quantity, StockMovementType.Purchase, createdByUserId, $"PurchaseInvoice#{invoice.Id}");
        }

        await db.SaveChangesAsync();

        if (amountPaid > 0)
        {
            if (request.PaymentMethod is not PaymentMethod method || method == PaymentMethod.Credit)
            {
                throw new ArgumentException("A cash/digital payment method is required when paying part of a purchase invoice.");
            }

            db.SupplierPayments.Add(new SupplierPayment
            {
                SupplierId = request.SupplierId,
                PurchaseInvoiceId = invoice.Id,
                Amount = amountPaid,
                Method = method,
                PaidByUserId = createdByUserId,
            });
            await db.SaveChangesAsync();

            await financial.PostAsync(
                method.ToAccountType(), FinancialTransactionType.SupplierPayment, TransactionDirection.Out,
                amountPaid, createdByUserId, $"PurchaseInvoice#{invoice.Id}");
        }

        if (invoice.OutstandingAmount > 0)
        {
            await financial.PostAsync(
                FinancialAccountType.SupplierPayables, FinancialTransactionType.SupplierInvoiceCredit, TransactionDirection.In,
                invoice.OutstandingAmount, createdByUserId, $"PurchaseInvoice#{invoice.Id}");
        }

        await dbTransaction.CommitAsync();
        return invoice;
    }

    public async Task<SupplierPayment> AddPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, string paidByUserId)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");
        }
        if (method == PaymentMethod.Credit)
        {
            throw new ArgumentException("Credit is not a valid settlement method for a supplier payment.", nameof(method));
        }

        var invoice = await db.PurchaseInvoices.FirstOrDefaultAsync(p => p.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Purchase invoice {invoiceId} not found.");

        if (amount > invoice.OutstandingAmount)
        {
            throw new InvalidOperationException("Payment amount exceeds the outstanding balance.");
        }

        using var dbTransaction = await db.Database.BeginTransactionAsync();

        var payment = new SupplierPayment
        {
            SupplierId = invoice.SupplierId,
            PurchaseInvoiceId = invoice.Id,
            Amount = amount,
            Method = method,
            PaidByUserId = paidByUserId,
        };
        db.SupplierPayments.Add(payment);

        invoice.AmountPaid += amount;
        invoice.OutstandingAmount -= amount;
        invoice.Status = invoice.OutstandingAmount <= 0 ? PurchaseInvoiceStatus.Paid : PurchaseInvoiceStatus.PartiallyPaid;

        await db.SaveChangesAsync();

        await financial.PostAsync(
            method.ToAccountType(), FinancialTransactionType.SupplierPayment, TransactionDirection.Out,
            amount, paidByUserId, $"PurchaseInvoice#{invoice.Id}");
        await financial.PostAsync(
            FinancialAccountType.SupplierPayables, FinancialTransactionType.SupplierPayment, TransactionDirection.Out,
            amount, paidByUserId, $"PurchaseInvoice#{invoice.Id}");

        await dbTransaction.CommitAsync();
        return payment;
    }
}
