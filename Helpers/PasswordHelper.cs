using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Helpers
{
    /// <summary>
    /// คลาสสำหรับจัดการความปลอดภัยของรหัสผ่าน (Password Hashing with Salt)
    /// ใช้เข้ารหัส SHA-256 ร่วมกับค่า Salt เพื่อป้องกันการโจมตีแบบ Rainbow Table
    /// </summary>
    public static class PasswordHelper
    {
        // ค่า Salt เฉพาะของระบบ LoveCat เพื่อเพิ่มความปลอดภัยในการแฮช
        private const string Salt = "_LoveCatSalt_2026!";

        /// <summary>
        /// ฟังก์ชันเข้ารหัสรหัสผ่าน (แปลงรหัสผ่านเป็น Hash String ก่อนบันทึกลง MySQL)
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Salt));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// ฟังก์ชันตรวจสอบความถูกต้องของรหัสผ่านตอนเข้าสู่ระบบ
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash)) return false;
            return HashPassword(password) == storedHash;
        }
    }
}
