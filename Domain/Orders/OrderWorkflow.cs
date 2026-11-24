using WebApplication1.Domain.Identity;

namespace WebApplication1.Domain.Orders;

/// <summary>
/// กำหนดว่าแต่ละบทบาทเปลี่ยนออเดอร์จากสถานะปัจจุบันไปเป็นสถานะใดได้
/// </summary>
public sealed class OrderWorkflow : IOrderWorkflow
{
    private sealed record Transition(OrderStatus From, OrderStatus To, string Role);

    private static readonly IReadOnlyList<Transition> Transitions =
    [
        new(OrderStatus.Pending, OrderStatus.Packing, Roles.Shop),
        new(OrderStatus.Packing, OrderStatus.Ready, Roles.Shop),
        new(OrderStatus.Ready, OrderStatus.Delivering, Roles.Raider),
        new(OrderStatus.Delivering, OrderStatus.Delivered, Roles.Raider),

        new(OrderStatus.Pending, OrderStatus.Cancelled, Roles.User),
        new(OrderStatus.Pending, OrderStatus.Cancelled, Roles.Shop),
        new(OrderStatus.Packing, OrderStatus.Cancelled, Roles.Shop)
    ];

    public bool CanTransition(OrderStatus from, OrderStatus to, string? actorRole) =>
        Transitions.Any(t => t.From == from && t.To == to && Roles.AreEqual(t.Role, actorRole));

    public string TrackingNoteFor(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "ร้านได้รับคำสั่งซื้อแล้ว รอยืนยันออเดอร์",
        OrderStatus.Packing => "ร้านกำลังแพ็กของให้น้อง",
        OrderStatus.Ready => "แพ็กเสร็จแล้ว รอไรเดอร์มารับ",
        OrderStatus.Delivering => "ไรเดอร์กำลังนำของไปส่งที่บ้านคุณ",
        OrderStatus.Delivered => "ส่งถึงแล้ว ขอบคุณที่อุดหนุน LoveCat",
        OrderStatus.Cancelled => "คำสั่งซื้อนี้ถูกยกเลิกแล้ว",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
