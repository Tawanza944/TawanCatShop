using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Options;

namespace WebApplication1.Services.Orders;

/// <summary>
/// หาเลขลำดับสูงสุดของวันแล้วบวกหนึ่ง โดยดัชนี unique ที่ orders.order_no ตรวจการชนของเลข
/// เมื่อคำขอพร้อมกันได้เลขเดียวกัน <see cref="CheckoutService"/> จะลองสร้างเลขใหม่
/// </summary>
public sealed class DailyOrderNumberGenerator : IOrderNumberGenerator
{
    private readonly AppDbContext _db;
    private readonly CommerceOptions _commerce;
    private readonly TimeProvider _clock;

    public DailyOrderNumberGenerator(AppDbContext db, IOptions<CommerceOptions> commerce, TimeProvider clock)
    {
        _db = db;
        _commerce = commerce.Value;
        _clock = clock;
    }

    public async Task<string> NextAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_commerce.TimeZoneId);
        var localToday = TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), timeZone);
        var prefix = $"{_commerce.OrderNumberPrefix}-{localToday.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-";

        var todaysNumbers = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderNo.StartsWith(prefix))
            .Select(o => o.OrderNo)
            .ToListAsync(cancellationToken);

        var last = todaysNumbers
            .Select(no => int.TryParse(no[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(last + 1).ToString("D4", CultureInfo.InvariantCulture)}";
    }
}
