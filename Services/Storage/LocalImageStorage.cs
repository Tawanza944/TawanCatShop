using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Storage;

/// <summary>
/// Stores uploaded images under App_Data/uploads/{folder} with a random file name and serves them at /uploads/{folder}/...
/// The folder is outside wwwroot on purpose: wwwroot holds the site's own assets, and dotnet watch restarts (and on
/// .NET 10 crashes) whenever a file appears there. Program.cs maps <see cref="RequestPath"/> to <see cref="RootFor"/>.
/// </summary>
public sealed class LocalImageStorage : IImageStorage
{
    public const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>URL prefix of every uploaded file; stored in the database as part of the image URL.</summary>
    public const string RequestPath = "/uploads";

    private static readonly IReadOnlyDictionary<string, string> AllowedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp"
    };

    private readonly string _root;
    private readonly ILogger<LocalImageStorage> _logger;

    public LocalImageStorage(IWebHostEnvironment environment, ILogger<LocalImageStorage> logger)
    {
        _root = RootFor(environment);
        _logger = logger;
    }

    /// <summary>Physical folder that holds uploads.</summary>
    public static string RootFor(IWebHostEnvironment environment) => Path.Combine(environment.ContentRootPath, "App_Data", "uploads");

    public async Task<OperationResult<string>> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return OperationResult<string>.Failure("ไฟล์รูปว่างเปล่า");
        }

        if (file.Length > MaxBytes)
        {
            return OperationResult<string>.Failure("รูปต้องมีขนาดไม่เกิน 5 MB");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.TryGetValue(extension, out var expectedType) || !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<string>.Failure("รองรับเฉพาะรูป JPG, PNG หรือ WebP");
        }

        var directory = Path.Combine(_root, folder);
        Directory.CreateDirectory(directory);

        // Never trust the uploaded file name: generate our own.
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await using (var stream = File.Create(Path.Combine(directory, fileName)))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return OperationResult<string>.Success($"{RequestPath}/{folder}/{fileName}");
    }

    public void Delete(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl) || !publicUrl.StartsWith(RequestPath + "/", StringComparison.Ordinal))
        {
            return;
        }

        // Resolve and make sure the path stays inside the uploads folder ("../" in a URL must not escape it).
        var root = Path.GetFullPath(_root) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(_root, publicUrl[(RequestPath.Length + 1)..]));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Could not delete {Path}.", path);
        }
    }
}
