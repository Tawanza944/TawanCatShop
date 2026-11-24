namespace WebApplication1.Services.Identity;

/// <summary>
/// ข้อมูลแบบอ่านอย่างเดียวของผู้ที่ล็อกอินในคำขอปัจจุบัน
/// Controller, Filter และ View Component อ่านข้อมูลผู้ใช้ผ่านบริการนี้
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? UserName { get; }

    string? DisplayName { get; }

    /// <summary>ตำแหน่งรูปโปรไฟล์ หากผู้ใช้ยังไม่เคยอัปโหลดจะเป็น null</summary>
    string? AvatarUrl { get; }

    /// <summary>บทบาทของผู้ใช้ โดยผู้ที่ล็อกอินและไม่มีบทบาทพนักงานใช้ <see cref="Domain.Identity.Roles.User"/></summary>
    string? Role { get; }

    bool IsInRole(params string[] roles);

    /// <summary>รหัสผู้ใช้ที่ล็อกอิน หากเรียกในคำขอของผู้ไม่ล็อกอินจะเกิดข้อผิดพลาด</summary>
    int RequireUserId();
}
