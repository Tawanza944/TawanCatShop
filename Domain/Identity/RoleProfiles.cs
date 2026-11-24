namespace WebApplication1.Domain.Identity;

/// <summary>ชื่อของแต่ละบทบาทที่แสดงบนหน้าเว็บและข้อมูลประกอบของเมนูบัญชี</summary>
public sealed record RoleProfile(string Role, string ThaiName, string WorkspaceTitle, string Icon, bool Shops);

public static class RoleProfiles
{
    private static readonly IReadOnlyDictionary<string, RoleProfile> Profiles =
        new Dictionary<string, RoleProfile>(StringComparer.OrdinalIgnoreCase)
        {
            [Roles.User] = new(Roles.User, "ลูกค้า", "คำสั่งซื้อของฉัน", "bi-bag-heart", Shops: true),
            [Roles.Admin] = new(Roles.Admin, "ผู้ดูแลระบบ", "หลังร้าน · ผู้ดูแล", "bi-shield-lock", Shops: false),
            [Roles.Shop] = new(Roles.Shop, "ร้านค้า", "หลังร้าน · ร้านค้า", "bi-shop", Shops: false),
            [Roles.Raider] = new(Roles.Raider, "ไรเดอร์", "งานส่งของ", "bi-bicycle", Shops: false),
            [Roles.Accounting] = new(Roles.Accounting, "ฝ่ายบัญชี", "หลังร้าน · บัญชี", "bi-calculator", Shops: false),
            [Roles.Owner] = new(Roles.Owner, "เจ้าของร้าน", "หลังร้าน · เจ้าของ", "bi-graph-up-arrow", Shops: false)
        };

    public static RoleProfile For(string? role) =>
        role is not null && Profiles.TryGetValue(role, out var profile) ? profile : Profiles[Roles.User];
}
