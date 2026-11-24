namespace WebApplication1.Domain.Shipping;

/// <summary>Parcel carriers a shop can choose when it ships an order outside the rider area.</summary>
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
/// Maps each <see cref="Carrier"/> to a stable code (stored in the database and posted by forms) and the name shown to people.
/// Codes never change once stored; display names can.
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

    /// <summary>Carriers in the order they appear in the shop's dropdown ("อื่น ๆ" last).</summary>
    public static IReadOnlyList<Carrier> All { get; } = Map.Keys.ToArray();

    public static string ToCode(this Carrier carrier) => Map[carrier].Code;

    public static string ToThai(this Carrier carrier) => Map[carrier].Name;

    /// <summary>Parses a posted or stored code. Case and surrounding spaces are ignored; unknown codes fail.</summary>
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
