namespace WebApplication1.Options;

/// <summary>ข้อมูลติดต่อที่แสดงในท้ายเว็บและหน้าช่วยเหลือ จากหัวข้อ StoreInfo ใน appsettings.json</summary>
public sealed class StoreInfoOptions
{
    public const string SectionName = "StoreInfo";

    public string Name { get; set; } = "LoveCat";

    public string Tagline { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string OpeningHours { get; set; } = string.Empty;

    public string Credit { get; set; } = string.Empty;

    public List<SocialAccount> Socials { get; set; } = [];
}

/// <summary>ข้อมูลบัญชีโซเชียล หากไม่มี <see cref="Url"/> จะแสดงชื่อบัญชีเป็นข้อความ</summary>
public sealed class SocialAccount
{
    public string Platform { get; set; } = string.Empty;

    public string Handle { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string? Url { get; set; }
}
