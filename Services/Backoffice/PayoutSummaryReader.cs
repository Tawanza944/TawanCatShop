using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Accounting;
using WebApplication1.Models;

namespace WebApplication1.Services.Backoffice;

public sealed record PayoutSummary(int Total, int AwaitingApproval, int Ready, int Transferred, decimal ReadyAmount)
{
    public int Unpaid => AwaitingApproval + Ready;
}

public sealed class PayoutSummaryReader
{
    public async Task<PayoutSummary> ReadAsync(IQueryable<Order> orders, CancellationToken token)
    {
        // สรุปจากผลค้นหาและสถานะจัดส่งก่อนกรองการเงิน จึงนับได้ครบทุกตัวเลือกใน dropdown
        var total = await orders.CountAsync(token);
        var awaiting = await orders.WherePayout(PayoutFilter.AwaitingApproval).CountAsync(token);
        var ready = orders.WherePayout(PayoutFilter.Ready);
        var readyCount = await ready.CountAsync(token);
        var readyAmount = await ready.SumAsync(o => (decimal?)o.ShopPayout!.NetAmount, token) ?? 0;
        var transferred = await orders.WherePayout(PayoutFilter.Transferred).CountAsync(token);
        return new PayoutSummary(total, awaiting, readyCount, transferred, readyAmount);
    }
}
