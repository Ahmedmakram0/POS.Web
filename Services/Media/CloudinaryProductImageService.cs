using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace POS.Web.Services.Media;

public class CloudinaryProductImageService(Cloudinary cloudinary) : IProductImageService
{
    private const string Folder = "pos-web/products";

    public async Task<ImageUploadResult> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = Folder,
            Overwrite = true,
            Transformation = new Transformation().Width(800).Height(800).Crop("limit").Quality("auto"),
        };

        var result = await cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        return new ImageUploadResult(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deletionParams = new DeletionParams(publicId);
        await cloudinary.DestroyAsync(deletionParams);
    }
}
