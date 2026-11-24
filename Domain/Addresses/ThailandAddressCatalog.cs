using System.Text.Json;

namespace WebApplication1.Domain.Addresses;

public sealed record AddressRegion(string ProvinceCode, string DistrictCode, string SubdistrictCode, string PostalCode);

public static class ThailandAddressCatalog
{
    private static readonly Lazy<Dictionary<string, Province>> Provinces = new(() =>
    {
        using var stream = typeof(ThailandAddressCatalog).Assembly.GetManifestResourceStream("ThailandAddresses.json")
            ?? throw new InvalidOperationException("Thailand address data is missing.");
        var provinces = JsonSerializer.Deserialize<Province[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Thailand address data is empty.");
        return provinces.ToDictionary(province => province.Code);
    });

    public static string? Format(AddressRegion? region)
    {
        if (region is null || string.IsNullOrWhiteSpace(region.ProvinceCode) ||
            !Provinces.Value.TryGetValue(region.ProvinceCode, out var province))
            return null;

        var district = province.Districts.SingleOrDefault(area => area.Code == region.DistrictCode);
        var subdistrict = district?.Subdistricts.SingleOrDefault(area => area.Code == region.SubdistrictCode);
        if (district is null || subdistrict is null || !subdistrict.PostalCodes.Contains(region.PostalCode))
            return null;

        return province.Code == "10"
            ? $"แขวง{subdistrict.Name} เขต{district.Name} {province.Name} {region.PostalCode}"
            : $"ต.{subdistrict.Name} อ.{district.Name} จ.{province.Name} {region.PostalCode}";
    }

    private sealed record Province(string Code, string Name, District[] Districts);
    private sealed record District(string Code, string Name, Subdistrict[] Subdistricts);
    private sealed record Subdistrict(string Code, string Name, string[] PostalCodes);
}
