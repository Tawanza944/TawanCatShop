using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Orders;
using WebApplication1.Domain.Pricing;
using WebApplication1.Models;

namespace WebApplication1.Services.Orders;

public static class SalesIncentives
{
    public static IQueryable<FlashSale> ActiveFlashSales(this AppDbContext db, DateTime now) =>
        db.Set<FlashSale>().Where(s => s.StartsAt <= now && s.EndsAt > now && s.Sold < s.Quota);

    public static async Task<LoyaltyBalance> LoyaltyBalanceAsync(this AppDbContext db, int customerId, CancellationToken token)
    {
        var amounts = await (from incentive in db.Set<OrderIncentives>().AsNoTracking()
                             join order in db.Orders.AsNoTracking() on incentive.OrderId equals order.Id
                             where order.UserId == customerId && order.Status != OrderStatus.Cancelled
                             select new { incentive.EligibleSpend, incentive.RewardUsed, order.Status })
            .ToListAsync(token);
        return LoyaltyRules.Balance(amounts.Where(a => a.Status == OrderStatus.Delivered).Sum(a => a.EligibleSpend),
            amounts.Sum(a => a.RewardUsed));
    }
}
