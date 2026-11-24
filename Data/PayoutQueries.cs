using WebApplication1.Domain.Accounting;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;

namespace WebApplication1.Data;

public static class PayoutQueries
{
    // กรองผ่าน IQueryable ก่อนแบ่งหน้า เพื่อให้ MySQL เลือกเฉพาะออเดอร์ที่ตรงเงื่อนไข
    public static IQueryable<Order> WherePayout(this IQueryable<Order> orders, PayoutFilter filter)
    {
        if (filter == PayoutFilter.All) return orders;
        var delivered = orders.Where(o => o.Status == OrderStatus.Delivered);
        return filter switch
        {
            PayoutFilter.Unpaid => delivered.Where(o => o.ShopPayout == null || o.ShopPayout.TransferDate == null),
            PayoutFilter.AwaitingApproval => delivered.Where(o => o.ShopPayout == null),
            PayoutFilter.Ready => delivered.Where(o => o.ShopPayout != null && o.ShopPayout.TransferDate == null),
            PayoutFilter.Transferred => delivered.Where(o => o.ShopPayout != null && o.ShopPayout.TransferDate != null),
            _ => orders
        };
    }
}
