using WebApplication1.Domain.Common;

namespace WebApplication1.Services.Storage;

/// <summary>บันทึกรูปและคืน URL โดยซ่อนรายละเอียดตำแหน่งที่จัดเก็บจากผู้เรียก</summary>
public interface IImageStorage
{
    /// <param name="folder">โฟลเดอร์ย่อยใต้ /uploads เช่น "products" หรือ "avatars"</param>
    Task<OperationResult<string>> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);

    void Delete(string? publicUrl);
}
