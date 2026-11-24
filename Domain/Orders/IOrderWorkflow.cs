namespace WebApplication1.Domain.Orders;

/// <summary>
/// กำหนดว่าแต่ละบทบาทเปลี่ยนออเดอร์จากสถานะหนึ่งไปอีกสถานะใดได้
/// </summary>
public interface IOrderWorkflow
{
    bool CanTransition(OrderStatus from, OrderStatus to, string? actorRole);

    /// <summary>ข้อความหน้าติดตามที่ลูกค้าเห็นเมื่อออเดอร์ถึง <paramref name="status"/></summary>
    string TrackingNoteFor(OrderStatus status);
}
