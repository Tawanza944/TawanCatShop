using WebApplication1.Models;

namespace WebApplication1.Services.Identity;

/// <summary>
/// เขียนและล้างข้อมูลผู้ใช้ในเซสชัน ส่วน <see cref="ICurrentUserService"/> ใช้อ่านค่าที่บันทึก
/// </summary>
public interface IAuthSession
{
    void SignIn(User user, string role);

    /// <summary>อ่านชื่อ ชื่อผู้ใช้ และอีเมลใหม่หลังผู้ใช้แก้โปรไฟล์</summary>
    void RefreshProfile(User user);

    void SetAvatar(string avatar);

    void SetRole(string role);

    void SignOut();
}
