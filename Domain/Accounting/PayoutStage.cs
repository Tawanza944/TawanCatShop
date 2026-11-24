using WebApplication1.Domain.Orders;

namespace WebApplication1.Domain.Accounting;

public enum PayoutStage
{
    AwaitingDelivery,
    AwaitingApproval,
    Ready,
    Transferred,
    Cancelled
}

public enum PayoutFilter
{
    All,
    Unpaid,
    AwaitingApproval,
    Ready,
    Transferred
}

public static class PayoutStages
{
    // สถานะจัดส่งเป็นเงื่อนไขแรกของการจ่ายเงิน แม้ข้อมูลโอนหรืออนุมัติจะมีอยู่ก็ตาม
    public static PayoutStage Resolve(OrderStatus delivery, bool approved, bool transferred) => delivery switch
    {
        OrderStatus.Cancelled => PayoutStage.Cancelled,
        not OrderStatus.Delivered => PayoutStage.AwaitingDelivery,
        _ when transferred => PayoutStage.Transferred,
        _ when approved => PayoutStage.Ready,
        _ => PayoutStage.AwaitingApproval
    };

    public static string Label(this PayoutStage stage) => stage switch
    {
        PayoutStage.Cancelled => "ยกเลิก",
        PayoutStage.AwaitingApproval => "รออนุมัติ",
        PayoutStage.Ready => "พร้อมโอน",
        PayoutStage.Transferred => "โอนแล้ว",
        _ => "รอส่งถึงลูกค้า"
    };
}
