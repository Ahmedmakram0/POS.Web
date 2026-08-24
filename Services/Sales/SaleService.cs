using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;
using POS.Web.Services.Catalog;
using POS.Web.Services.Financial;

namespace POS.Web.Services.Sales;

public class SaleService(ApplicationDbContext db, IInventoryService inventory, IFinancialAccountService financial)
    : ISaleService
{
    public async Task<List<Sale>> GetAllAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var query = db.Sales.Include(s => s.Customer).Include(s => s.Items).AsQueryable();

        if (fromUtc is DateTime from)
        {
            query = query.Where(s => s.CreatedAt >= from);
        }
        if (toUtc is DateTime to)
        {
            query = query.Where(s => s.CreatedAt <= to);
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public Task<Sale?> GetByIdAsync(int id) =>
        db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Sale> CreateAsync(CreateSaleRequest request, string cashierUserId)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("A sale must contain at least one item.");
        }
        if (request.Payments.Count == 0)
        {
            throw new InvalidOperationException("A sale must have at least one payment entry.");
        }

        var creditAmount = request.Payments.Where(p => p.Method == PaymentMethod.Credit).Sum(p => p.Amount);
        var nonCreditPaid = request.Payments.Where(p => p.Method != PaymentMethod.Credit).Sum(p => p.Amount);

        if (creditAmount > 0 && request.CustomerId is null)
        {
            throw new InvalidOperationException("A customer is required for a sale that includes a credit payment.");
        }

        var lineItems = new List<SaleItem>();
        decimal subtotal = 0;
        decimal totalDiscount = 0;

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException("Item quantity must be positive.");
            }

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId)
                ?? throw new KeyNotFoundException($"Product {line.ProductId} not found.");

            var originalUnitPrice = product.SellingPrice;
            var finalUnitPrice = line.DiscountType switch
            {
                null => originalUnitPrice,
                DiscountType.FixedAmount => originalUnitPrice - line.DiscountValue,
                DiscountType.Percentage => originalUnitPrice * (1 - line.DiscountValue / 100m),
                DiscountType.FinalUnitPrice => line.DiscountValue,
                _ => throw new ArgumentOutOfRangeException(nameof(line)),
            };
            finalUnitPrice = Math.Max(0, finalUnitPrice);
            var discountPerUnit = originalUnitPrice - finalUnitPrice;

            var saleItem = new SaleItem
            {
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                ProductBarcodeSnapshot = product.Barcode,
                Quantity = line.Quantity,
                OriginalUnitPrice = originalUnitPrice,
                DiscountType = line.DiscountType,
                DiscountValue = line.DiscountValue,
                DiscountAmountPerUnit = discountPerUnit,
                FinalUnitPrice = finalUnitPrice,
                MinimumSellingPriceSnapshot = product.MinimumSellingPrice,
                MinimumPriceOverridden = finalUnitPrice < product.MinimumSellingPrice,
                LineTotal = finalUnitPrice * line.Quantity,
            };
            lineItems.Add(saleItem);

            subtotal += originalUnitPrice * line.Quantity;
            totalDiscount += discountPerUnit * line.Quantity;
        }

        var total = subtotal - totalDiscount;
        var amountDueAfterCredit = total - creditAmount;

        if (nonCreditPaid < amountDueAfterCredit)
        {
            throw new InvalidOperationException("Payments do not cover the sale total.");
        }

        var changeGiven = nonCreditPaid - amountDueAfterCredit;

        using var dbTransaction = await db.Database.BeginTransactionAsync();

        var today = DateTime.UtcNow.Date;
        var todaysCount = await db.Sales.CountAsync(s => s.CreatedAt >= today);
        var invoiceNumber = $"S-{today:yyyyMMdd}-{todaysCount + 1:0000}";

        var sale = new Sale
        {
            InvoiceNumber = invoiceNumber,
            CashierUserId = cashierUserId,
            CustomerId = request.CustomerId,
            SubtotalBeforeDiscount = subtotal,
            TotalDiscount = totalDiscount,
            Total = total,
            AmountPaid = nonCreditPaid,
            ChangeGiven = changeGiven,
            CreditAmount = creditAmount,
        };
        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        foreach (var item in lineItems)
        {
            item.SaleId = sale.Id;
            db.SaleItems.Add(item);
        }
        foreach (var payment in request.Payments)
        {
            db.Payments.Add(new Payment { SaleId = sale.Id, Method = payment.Method, Amount = payment.Amount });
        }
        await db.SaveChangesAsync();

        foreach (var item in lineItems)
        {
            await inventory.AdjustStockAsync(
                item.ProductId, -item.Quantity, StockMovementType.Sale, cashierUserId, $"Sale#{sale.Id}");
        }

        var reference = $"Sale#{sale.Id}";
        foreach (var payment in request.Payments.Where(p => p.Method != PaymentMethod.Credit && p.Amount > 0))
        {
            var accountType = payment.Method switch
            {
                PaymentMethod.Cash => FinancialAccountType.CashSafe,
                PaymentMethod.InstaPay => FinancialAccountType.InstaPay,
                PaymentMethod.VodafoneCash => FinancialAccountType.VodafoneCash,
                _ => throw new ArgumentOutOfRangeException(nameof(request)),
            };
            var transactionType = payment.Method == PaymentMethod.Cash
                ? FinancialTransactionType.CashSale
                : FinancialTransactionType.DigitalSale;

            await financial.PostAsync(accountType, transactionType, TransactionDirection.In, payment.Amount, cashierUserId, reference);
        }

        if (creditAmount > 0)
        {
            var customerId = request.CustomerId!.Value;
            var previousBalance = await db.CustomerCreditTransactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Select(t => t.BalanceAfter)
                .FirstOrDefaultAsync();

            db.CustomerCreditTransactions.Add(new CustomerCreditTransaction
            {
                CustomerId = customerId,
                Type = CustomerCreditTransactionType.CreditSale,
                Amount = creditAmount,
                BalanceAfter = previousBalance + creditAmount,
                SaleId = sale.Id,
                RecordedByUserId = cashierUserId,
            });
            await db.SaveChangesAsync();

            await financial.PostAsync(
                FinancialAccountType.CustomerReceivables, FinancialTransactionType.CreditSale,
                TransactionDirection.In, creditAmount, cashierUserId, reference);
        }

        await dbTransaction.CommitAsync();

        return (await GetByIdAsync(sale.Id))!;
    }

    public async Task<Sale> VoidAsync(int saleId, string voidedByUserId, string reason)
    {
        var sale = await db.Sales.Include(s => s.Items).Include(s => s.Payments).FirstOrDefaultAsync(s => s.Id == saleId)
            ?? throw new KeyNotFoundException($"Sale {saleId} not found.");

        if (sale.Status == SaleStatus.Voided)
        {
            throw new InvalidOperationException("Sale is already voided.");
        }

        using var dbTransaction = await db.Database.BeginTransactionAsync();

        sale.Status = SaleStatus.Voided;
        sale.VoidedByUserId = voidedByUserId;
        sale.VoidedAt = DateTime.UtcNow;
        sale.VoidReason = reason;
        await db.SaveChangesAsync();

        var reference = $"VoidSale#{sale.Id}";

        foreach (var item in sale.Items)
        {
            await inventory.AdjustStockAsync(item.ProductId, item.Quantity, StockMovementType.Correction, voidedByUserId, reference, reason);
        }

        foreach (var payment in sale.Payments.Where(p => p.Method != PaymentMethod.Credit && p.Amount > 0))
        {
            var accountType = payment.Method switch
            {
                PaymentMethod.Cash => FinancialAccountType.CashSafe,
                PaymentMethod.InstaPay => FinancialAccountType.InstaPay,
                PaymentMethod.VodafoneCash => FinancialAccountType.VodafoneCash,
                _ => throw new ArgumentOutOfRangeException(nameof(sale)),
            };
            var transactionType = payment.Method == PaymentMethod.Cash
                ? FinancialTransactionType.CashSale
                : FinancialTransactionType.DigitalSale;

            await financial.PostAsync(accountType, transactionType, TransactionDirection.Out, payment.Amount, voidedByUserId, reference, reason);
        }

        if (sale.CreditAmount > 0 && sale.CustomerId is int customerId)
        {
            var previousBalance = await db.CustomerCreditTransactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Select(t => t.BalanceAfter)
                .FirstOrDefaultAsync();

            db.CustomerCreditTransactions.Add(new CustomerCreditTransaction
            {
                CustomerId = customerId,
                Type = CustomerCreditTransactionType.Payment,
                Amount = sale.CreditAmount,
                BalanceAfter = previousBalance - sale.CreditAmount,
                SaleId = sale.Id,
                RecordedByUserId = voidedByUserId,
            });
            await db.SaveChangesAsync();

            await financial.PostAsync(
                FinancialAccountType.CustomerReceivables, FinancialTransactionType.CreditSale,
                TransactionDirection.Out, sale.CreditAmount, voidedByUserId, reference, reason);
        }

        await dbTransaction.CommitAsync();

        return (await GetByIdAsync(sale.Id))!;
    }
}
