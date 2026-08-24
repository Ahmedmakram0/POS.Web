namespace POS.Web.Services.Media;

public record ImageUploadResult(string Url, string PublicId);

public interface IProductImageService
{
    Task<ImageUploadResult> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}
