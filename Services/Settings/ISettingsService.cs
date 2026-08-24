using Microsoft.AspNetCore.Http;
using POS.Web.Models.Entities;

namespace POS.Web.Services.Settings;

public interface ISettingsService
{
    Task<SystemSettings> GetSystemSettingsAsync();
    Task<ReceiptSettings> GetReceiptSettingsAsync();
    Task<DiscountSettings> GetDiscountSettingsAsync();

    Task SaveSystemSettingsAsync(SystemSettings model);

    /// <summary>Saves receipt settings; when logoFile is provided, uploads it to wwwroot/uploads and deletes the previous logo.</summary>
    /// <returns>An error message if the logo file was rejected, otherwise null.</returns>
    Task<string?> SaveReceiptSettingsAsync(ReceiptSettings model, IFormFile? logoFile);

    Task SaveDiscountSettingsAsync(DiscountSettings model);
}
