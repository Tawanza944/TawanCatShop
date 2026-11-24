using System.Text.RegularExpressions;
using WebApplication1.Domain.Common;

namespace WebApplication1.Domain.Shipping;

/// <summary>
/// ชื่อผู้ให้บริการขนส่งและเลขติดตามที่ร้านกรอกเมื่อส่งมอบพัสดุ
/// เป็น Value Object ที่สร้างผ่าน <see cref="Create"/> เพื่อให้ทุกอินสแตนซ์มีค่าที่ผ่านการตรวจ
/// ส่วนอื่นจึงใช้ชื่อขนส่งและเลขติดตามจากออบเจ็กต์นี้ได้โดยตรง
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

    /// <summary>ชื่อขนส่งที่ร้านกรอกเมื่อ <see cref="Carrier"/> เป็น <see cref="Shipping.Carrier.Other"/></summary>
    public string? OtherCarrierName { get; }

    /// <summary>เลขติดตามที่ปรับเป็นตัวพิมพ์ใหญ่และตัดช่องว่างกับขีดออกแล้ว</summary>
    public string TrackingNumber { get; }

    /// <summary>ชื่อขนส่งที่แสดงให้ลูกค้าและพนักงานเห็น</summary>
    public string CarrierDisplayName => Carrier == Carrier.Other && OtherCarrierName is { } name ? name : Carrier.ToThai();

    /// <summary>
    /// ตรวจค่าจากฟอร์มและตัดช่องว่างกับขีดออกจากเลขติดตามเพื่อรองรับข้อความที่คัดลอกมา
    /// ตัวอย่าง "EB 1234-5678 9 TH" หลังปรับรูปแบบต้องเหลือตัวอักษรอังกฤษหรือตัวเลข 8–30 ตัว
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

    /// <summary>สร้างออบเจ็กต์จากค่าที่ผ่านการตรวจและบันทึกไว้ในฐานข้อมูลแล้ว</summary>
    public static ShipmentDetails FromStored(Carrier carrier, string? otherCarrierName, string trackingNumber) =>
        new(carrier, carrier == Carrier.Other ? otherCarrierName : null, trackingNumber);

    [GeneratedRegex(@"[\s\-]+")]
    private static partial Regex Separators();

    // จำนวนตัวอักษรในนิพจน์ปกติต้องตรงกับ TrackingMinLength และ TrackingMaxLength
    [GeneratedRegex(@"^[A-Z0-9]{8,30}$")]
    private static partial Regex TrackingPattern();
}
