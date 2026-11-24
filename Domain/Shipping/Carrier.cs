namespace WebApplication1.Domain.Shipping;

/// <summary>ผู้ให้บริการขนส่งที่ร้านเลือกได้สำหรับออเดอร์นอกพื้นที่ไรเดอร์</summary>
public enum Carrier
{
    ThailandPostEms,
    ThailandPostRegistered,
    KerryExpress,
    FlashExpress,
    JtExpress,
    ScgExpress,
    BestExpress,
    Other
}

/// <summary>
/// กำหนดรหัสคงที่ของ <see cref="Carrier"/> สำหรับฐานข้อมูลและฟอร์ม พร้อมชื่อที่ใช้แสดง
/// คงรหัสที่บันทึกไว้เพื่อรองรับข้อมูลเก่า ส่วนชื่อแสดงผลปรับได้
/// </summary>
public static class Carriers
{
    private static readonly IReadOnlyDictionary<Carrier, (string Code, string Name)> Map =
        new Dictionary<Carrier, (string, string)>
        {
            [Carrier.ThailandPostEms] = ("thaipost-ems", "ไปรษณีย์ไทย (EMS)"),
            [Carrier.ThailandPostRegistered] = ("thaipost-registered", "ไปรษณีย์ไทย (ลงทะเบียน)"),
            [Carrier.KerryExpress] = ("kerry", "Kerry Express"),
            [Carrier.FlashExpress] = ("flash", "Flash Express"),
            [Carrier.JtExpress] = ("jt", "J&T Express"),
            [Carrier.ScgExpress] = ("scg", "SCG Express"),
            [Carrier.BestExpress] = ("best", "Best Express"),
            [Carrier.Other] = ("other", "อื่น ๆ")
        };

    /// <summary>เรียงตัวเลือกขนส่งตามลำดับในฟอร์มร้าน โดยให้ “อื่น ๆ” อยู่ท้ายรายการ</summary>
    public static IReadOnlyList<Carrier> All { get; } = Map.Keys.ToArray();

    public static string ToCode(this Carrier carrier) => Map[carrier].Code;

    public static string ToThai(this Carrier carrier) => Map[carrier].Name;

    /// <summary>อ่านรหัสจากฟอร์มหรือฐานข้อมูลโดยตัดช่องว่างและเทียบแบบไม่สนตัวพิมพ์ หากไม่รู้จักรหัสจะคืนผลไม่สำเร็จ</summary>
    public static bool TryParseCode(string? code, out Carrier carrier)
    {
        var normalized = code?.Trim();
        foreach (var (key, value) in Map)
        {
            if (string.Equals(value.Code, normalized, StringComparison.OrdinalIgnoreCase))
            {
                carrier = key;
                return true;
            }
        }

        carrier = default;
        return false;
    }
}
