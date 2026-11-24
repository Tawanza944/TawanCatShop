using System.Security.Cryptography;
using WebApplication1.Models;

namespace WebApplication1.Services.Orders;

public static class SlipImage
{
    public const int MaxBytes = 2 * 1024 * 1024;
    public const int PayoutMaxBytes = 10 * 1024 * 1024;

    public static async Task<BankSlip?> ReadAsync(IFormFile? file, CancellationToken token, int maxBytes = MaxBytes)
    {
        if (file is null || file.Length <= 0 || file.Length > maxBytes)
            return null;
        using var buffer = new MemoryStream((int)file.Length);
        await file.CopyToAsync(buffer, token);
        return FromBytes(buffer.ToArray());
    }

    public static BankSlip? Decode(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64) || base64.Length > ((MaxBytes + 2) / 3) * 4)
            return null;

        byte[] bytes;
        try { bytes = Convert.FromBase64String(base64); }
        catch (FormatException) { return null; }
        if (bytes.Length > MaxBytes)
            return null;

        return FromBytes(bytes);
    }

    private static BankSlip? FromBytes(byte[] bytes)
    {
        var contentType = bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ? "image/png" :
            bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 }) ? "image/jpeg" : null;
        return contentType is null ? null : new BankSlip { Id = Guid.NewGuid(), Checksum = Convert.ToHexString(SHA256.HashData(bytes)), ContentType = contentType, Content = bytes };
    }
}
