using WebApplication1.Domain.Common;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Services.Orders;

/// <summary>
/// จัดการออเดอร์หลังสั่งซื้อ เช่น ร้านแพ็ก และไรเดอร์รับงานกับส่งสำเร็จ
/// การดำเนินการรวมถึงการยกเลิกจะตรวจตามบัญชีที่ล็อกอิน
/// </summary>
public interface IOrderFulfillmentService
{
    Task<OperationResult<OrderStatus>> MoveToAsync(string orderNo, OrderStatus nextStatus, CancellationToken cancellationToken = default);

    /// <summary>ยกเลิกออเดอร์ที่เข้าเงื่อนไข คืนสต็อกและสิทธิ์คูปองให้ลูกค้า</summary>
    Task<OperationResult<bool>> CancelAsync(string orderNo, CancellationToken cancellationToken = default);
}
