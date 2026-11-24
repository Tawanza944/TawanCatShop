using WebApplication1.Domain.Shipping;

namespace WebApplication1.Models;

/// <summary>
/// ข้อมูลพัสดุใน <c>order_shipments</c> สำหรับออเดอร์นอกพื้นที่ไรเดอร์ที่ร้านส่งผ่านผู้ให้บริการขนส่ง
/// บันทึกครั้งเดียวผ่าน <see cref="Services.Orders.ShipmentService"/> ข้อมูลการส่งเดิมจะถูกเก็บไว้
/// </summary>
public sealed class OrderShipment
{
    public int OrderId { get; set; }

    public Carrier Carrier { get; set; }

    /// <summary>ชื่อขนส่งที่ร้านกรอกเมื่อเลือก <see cref="Carrier"/> เป็น <see cref="Carrier.Other"/></summary>
    public string? OtherCarrierName { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;

    public int ShippedBy { get; set; }

    /// <summary>เวลาที่บันทึกการส่งในเขตเวลา UTC</summary>
    public DateTime ShippedAt { get; set; }

    /// <summary>แปลงข้อมูลที่จัดเก็บเป็นรายละเอียดขนส่งและเลขติดตามในรูปแบบที่ผ่านการตรวจแล้ว</summary>
    public ShipmentDetails Details => ShipmentDetails.FromStored(Carrier, OtherCarrierName, TrackingNumber);
}
