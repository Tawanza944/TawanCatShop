using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication1.Domain.Shipping;

namespace WebApplication1.Data.Converters;

/// <summary>Stores <see cref="Carrier"/> as its stable code ('flash', 'thaipost-ems', ...) instead of an enum number.</summary>
public sealed class CarrierConverter : ValueConverter<Carrier, string>
{
    public CarrierConverter()
        : base(carrier => carrier.ToCode(), code => Parse(code))
    {
    }

    private static Carrier Parse(string code) =>
        Carriers.TryParseCode(code, out var carrier) ? carrier : throw new InvalidOperationException($"Unknown carrier code '{code}' in order_shipments.");
}
