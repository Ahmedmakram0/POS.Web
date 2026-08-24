using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;

namespace POS.Web.Services.Settings;

public class SettingsService(ApplicationDbContext db, IWebHostEnvironment env) : ISettingsService
{
    private static readonly string[] AllowedLogoExtensions = [".png", ".jpg", ".jpeg", ".webp", ".svg"];

    public Task<SystemSettings> GetSystemSettingsAsync() => GetOrCreateAsync(db.SystemSettings);
    public Task<ReceiptSettings> GetReceiptSettingsAsync() => GetOrCreateAsync(db.ReceiptSettings);
    public Task<DiscountSettings> GetDiscountSettingsAsync() => GetOrCreateAsync(db.DiscountSettings);

    public async Task SaveSystemSettingsAsync(SystemSettings model)
    {
        var settings = await GetSystemSettingsAsync();
        settings.StoreName = model.StoreName;
        settings.StoreAddress = model.StoreAddress;
        settings.StorePhone = model.StorePhone;
        settings.Currency = model.Currency;
        settings.DefaultLowStockThreshold = model.DefaultLowStockThreshold;
        await db.SaveChangesAsync();
    }

    public async Task<string?> SaveReceiptSettingsAsync(ReceiptSettings model, IFormFile? logoFile)
    {
        var settings = await GetReceiptSettingsAsync();
        settings.ReceiptTitle = model.ReceiptTitle;
        settings.TaxInfo = model.TaxInfo;
        settings.FooterMessage = model.FooterMessage;
        settings.ReceiptWidthMm = model.ReceiptWidthMm is 58 or 80 ? model.ReceiptWidthMm : 80;

        if (logoFile is { Length: > 0 })
        {
            var extension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            if (!AllowedLogoExtensions.Contains(extension))
            {
                return "صيغة الصورة غير مدعومة. الصيغ المسموحة: PNG, JPG, WEBP, SVG.";
            }

            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            if (!string.IsNullOrEmpty(settings.LogoUrl))
            {
                var oldPath = Path.Combine(env.WebRootPath, settings.LogoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            var fileName = $"logo-{Guid.NewGuid():N}{extension}";
            await using (var stream = File.Create(Path.Combine(uploadsDir, fileName)))
            {
                await logoFile.CopyToAsync(stream);
            }

            settings.LogoUrl = $"/uploads/{fileName}";
        }

        await db.SaveChangesAsync();
        return null;
    }

    public async Task SaveDiscountSettingsAsync(DiscountSettings model)
    {
        var settings = await GetDiscountSettingsAsync();
        settings.DefaultCashierMaxDiscountPercent = model.DefaultCashierMaxDiscountPercent;
        settings.RequireManagerApprovalAboveLimit = model.RequireManagerApprovalAboveLimit;
        await db.SaveChangesAsync();
    }

    private async Task<T> GetOrCreateAsync<T>(DbSet<T> set) where T : class, new()
    {
        var entity = await set.FirstOrDefaultAsync();
        if (entity is not null)
        {
            return entity;
        }

        entity = new T();
        set.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }
}
