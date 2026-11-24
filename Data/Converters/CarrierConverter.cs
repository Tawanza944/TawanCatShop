using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication1.Domain.Shipping;

namespace WebApplication1.Data.Converters;

/// <summary>บันทึก <see cref="Carrier"/> เป็นรหัสคงที่ เช่น 'flash' หรือ 'thaipost-ems' เพื่อให้ค่าในฐานอ่านได้ตรงกัน</summary>
public sealed class CarrierConverter : ValueConverter<Carrier, string>
{
    public CarrierConverter()
        : base(carrier => carrier.ToCode(), code => Parse(code))
    {
    }

    private static Carrier Parse(string code) =>
        Carriers.TryParseCode(code, out var carrier) ? carrier : throw new InvalidOperationException($"Unknown carrier code '{code}' in order_shipments.");
}
