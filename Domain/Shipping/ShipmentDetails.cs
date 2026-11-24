using System.Text.RegularExpressions;
using WebApplication1.Domain.Common;

namespace WebApplication1.Domain.Shipping;

/// <summary>
/// The carrier and tracking number a shop enters when it hands a parcel to a carrier.
/// Value object: it can only be created through <see cref="Create"/>, so an instance is always valid
/// (the rest of the system never has to re-check the tracking number or the carrier name).
/// </summary>
public sealed partial record ShipmentDetails
{
    public const int TrackingMinLength = 8;
    public const int TrackingMaxLength = 30;
    public const int CarrierNameMinLength = 2;
    public const int CarrierNameMaxLength = 50;

    private ShipmentDetails(Carrier carrier, string? otherCarrierName, string trackingNumber)
    {
        Carrier = carrier;
        OtherCarrierName = otherCarrierName;
        TrackingNumber = trackingNumber;
    }

    public Carrier Carrier { get; }

    /// <summary>Name typed by the shop; only set when <see cref="Carrier"/> is <see cref="Shipping.Carrier.Other"/>.</summary>
    public string? OtherCarrierName { get; }

    /// <summary>Normalized tracking number: upper case, no spaces or dashes.</summary>
    public string TrackingNumber { get; }

    /// <summary>Carrier name to show customers and staff.</summary>
    public string CarrierDisplayName => Carrier == Carrier.Other && OtherCarrierName is { } name ? name : Carrier.ToThai();

    /// <summary>
    /// Validates raw form input. Spaces and dashes inside the tracking number are removed because people paste numbers
    /// like "EB 1234-5678 9 TH"; what is left must be 8–30 English letters or digits.
    /// </summary>
    public static OperationResult<ShipmentDetails> Create(string? carrierCode, string? otherCarrierName, string? trackingNumber)
    {
        if (!Carriers.TryParseCode(carrierCode, out var carrier))
            return OperationResult<ShipmentDetails>.Failure("เลือกบริษัทขนส่ง");

        string? name = null;
        if (carrier == Carrier.Other)
        {
            name = otherCarrierName?.Trim();
            if (name is null || name.Length < CarrierNameMinLength || name.Length > CarrierNameMaxLength)
                return OperationResult<ShipmentDetails>.Failure($"พิมพ์ชื่อบริษัทขนส่ง {CarrierNameMinLength}–{CarrierNameMaxLength} ตัวอักษร");
        }

        var number = Separators().Replace(trackingNumber ?? string.Empty, string.Empty).ToUpperInvariant();
        if (number.Length == 0)
            return OperationResult<ShipmentDetails>.Failure("กรอกเลขพัสดุ");
        if (!TrackingPattern().IsMatch(number))
            return OperationResult<ShipmentDetails>.Failure(
                $"เลขพัสดุต้องเป็นตัวอักษรอังกฤษหรือตัวเลข {TrackingMinLength}–{TrackingMaxLength} ตัว");

        return OperationResult<ShipmentDetails>.Success(new ShipmentDetails(carrier, name, number));
    }

    /// <summary>Rebuilds an instance from values already validated and stored in the database.</summary>
    public static ShipmentDetails FromStored(Carrier carrier, string? otherCarrierName, string trackingNumber) =>
        new(carrier, carrier == Carrier.Other ? otherCarrierName : null, trackingNumber);

    [GeneratedRegex(@"[\s\-]+")]
    private static partial Regex Separators();

    // Must match TrackingMinLength / TrackingMaxLength (a const regex cannot be built from int constants).
    [GeneratedRegex(@"^[A-Z0-9]{8,30}$")]
    private static partial Regex TrackingPattern();
}
