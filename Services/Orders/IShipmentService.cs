using WebApplication1.Domain.Common;
using WebApplication1.Domain.Shipping;

namespace WebApplication1.Services.Orders;

/// <summary>บันทึกการจัดส่งผ่านผู้ให้บริการขนส่งสำหรับออเดอร์นอกพื้นที่ไรเดอร์</summary>
public interface IShipmentService
{
    /// <summary>
    /// บันทึกบริษัทขนส่งและเลขติดตามของร้านที่ล็อกอิน พร้อมเปลี่ยนเป็นกำลังจัดส่งในธุรกรรมเดียวกัน
    /// บันทึกได้ครั้งเดียวต่อออเดอร์
    /// </summary>
    Task<OperationResult<bool>> DispatchAsync(string orderNo, ShipmentDetails shipment, CancellationToken cancellationToken = default);
}
