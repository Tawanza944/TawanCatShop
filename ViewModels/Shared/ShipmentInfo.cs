using WebApplication1.Domain.Orders;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Shared;

/// <summary>รูปแบบข้อมูลการจัดส่งที่หน้าเว็บจะแสดง</summary>
public enum ShipmentDisplay
{
    /// <summary>ร้านบันทึกชื่อขนส่งและเลขติดตามแล้ว</summary>
    Tracked,

    /// <summary>ออเดอร์ที่ส่งก่อนมีระบบเลขติดตาม จึงไม่มีแถวใน order_shipments</summary>
    LegacyWithoutTracking
}

/// <summary>
/// เตรียมข้อมูลพัสดุให้การ์ดออเดอร์ของลูกค้าและรายการของฝ่ายบัญชี
/// ใช้กติกาการแสดงผลร่วมกันเพื่อให้ทั้งสองหน้าแสดงข้อมูลตรงกัน
/// </summary>
public sealed record ShipmentInfo(ShipmentDisplay Display, string? CarrierName, string? TrackingNumber, DateTime? ShippedAt)
{
    /// <summary>
    /// คืน null สำหรับออเดอร์ไรเดอร์ ออเดอร์ที่ไม่มีข้อมูลร้าน ออเดอร์ยกเลิก
    /// และออเดอร์พัสดุที่ร้านยังไม่ได้ส่ง
    /// </summary>
    public static ShipmentInfo? For(bool? riderDelivery, OrderStatus status, OrderShipment? shipment)
    {
        if (riderDelivery is not false)
            return null;

        if (shipment is not null)
        {
            var details = shipment.Details;
            return new ShipmentInfo(ShipmentDisplay.Tracked, details.CarrierDisplayName, details.TrackingNumber, shipment.ShippedAt);
        }

        return status is OrderStatus.Delivering or OrderStatus.Delivered
            ? new ShipmentInfo(ShipmentDisplay.LegacyWithoutTracking, null, null, null)
            : null;
    }

    public static ShipmentInfo? For(Order order) => For(order.Marketplace?.RiderDelivery, order.Status, order.Shipment);
}
