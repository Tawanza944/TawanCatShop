using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.Backoffice;

public sealed record FlashProduct(int Id, string Name, string Shop, decimal Price);
public sealed record FlashListing(string Product, string Shop, decimal Price, DateTime StartsAt, DateTime EndsAt, int Quota, int Sold);
public sealed record FlashAdminPage(IReadOnlyList<FlashProduct> Products, IReadOnlyList<FlashListing> Sales, DateTime Now);
public sealed record FlashSaleDraft(string ProductId, string Price, string StartsAt, string EndsAt, string Quota);

public sealed class FlashSaleAdmin(AppDbContext db, TimeProvider clock)
{
    public async Task<OperationResult<bool>> CreateAsync(int productId, decimal price, DateTime start, DateTime end, int quota, CancellationToken token)
    {
        if (productId <= 0) return OperationResult.Fail("เลือกสินค้าก่อนสร้าง Flash Sale");
        if (price <= 0 || decimal.Round(price, 2) != price) return OperationResult.Fail("ราคาพิเศษต้องมากกว่า ฿0 และมีทศนิยมไม่เกิน 2 ตำแหน่ง");
        if (start >= end) return OperationResult.Fail("เวลาสิ้นสุดต้องอยู่หลังเวลาเริ่ม");
        if (end <= clock.GetUtcNow().UtcDateTime) return OperationResult.Fail("เวลาสิ้นสุดต้องยังไม่ผ่านไป");
        if (quota <= 0) return OperationResult.Fail("โควตาต้องมีอย่างน้อย 1 ชิ้น");

        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await db.Products.Where(p => p.Id == productId)
            .ExecuteUpdateAsync(update => update.SetProperty(p => p.Name, p => p.Name), token);
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == productId && p.Shop != null, token);
        if (product is null) return OperationResult.Fail("ไม่พบสินค้าที่พร้อมตั้ง Flash Sale");
        if (price >= product.Price) return OperationResult.Fail("ราคาพิเศษต้องต่ำกว่าราคาขายของสินค้า");
        if (quota > product.Stock) return OperationResult.Fail($"โควตาเกินสต็อกปัจจุบัน ({product.Stock} ชิ้น)");
        if (await db.Set<FlashSale>().AnyAsync(s => s.ProductId == productId && s.StartsAt < end && s.EndsAt > start, token))
            return OperationResult.Fail("สินค้านี้มี Flash Sale ในช่วงเวลาที่ทับซ้อนกัน");

        db.Add(new FlashSale { ProductId = productId, Price = price, StartsAt = start, EndsAt = end, Quota = quota });
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return OperationResult.Ok();
    }
}
