using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Storage;

/// <summary>Saves uploaded images and returns the public URL. Hides where files live (disk today, cloud later).</summary>
public interface IImageStorage
{
    /// <param name="folder">Sub-folder under /uploads, e.g. "products" or "avatars".</param>
    Task<OperationResult<string>> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);

    void Delete(string? publicUrl);
}
