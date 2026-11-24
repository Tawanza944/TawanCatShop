using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Storage;

/// <summary>
/// เก็บรูปใน App_Data/uploads/{folder} ด้วยชื่อสุ่มและเปิดให้อ่านผ่าน /uploads/{folder}/...
/// แยกรูปอัปโหลดจากไฟล์เว็บใน wwwroot เพื่อให้ dotnet watch ไม่โหลดแอปใหม่เพราะรูปที่อัปโหลด
/// Program.cs เชื่อม <see cref="RequestPath"/> กับโฟลเดอร์จาก <see cref="RootFor"/>
/// </summary>
public sealed class LocalImageStorage : IImageStorage
{
    public const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>ส่วนต้นของ URL รูปที่อัปโหลด ซึ่งจัดเก็บเป็นส่วนหนึ่งของ URL รูปในฐานข้อมูล</summary>
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

    /// <summary>โฟลเดอร์บนดิสก์ที่ใช้เก็บไฟล์อัปโหลด</summary>
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

        // สุ่มชื่อไฟล์ฝั่งเซิร์ฟเวอร์เพื่อควบคุมชื่อที่จัดเก็บ
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

        // ตรวจ path หลังแปลงเป็นตำแหน่งเต็ม เพื่อให้ยังอยู่ใต้โฟลเดอร์เก็บไฟล์อัปโหลด
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
