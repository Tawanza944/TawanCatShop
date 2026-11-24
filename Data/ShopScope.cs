using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public static class ShopScope
{
    // ตรวจสมาชิกของร้านจากบัญชีปัจจุบัน แทนการเชื่อรหัสร้านที่ส่งมาจากฟอร์ม
    public static Task<int?> ShopIdAsync(this AppDbContext db, int userId, CancellationToken token) =>
        db.Set<ShopMembership>().Where(m => m.UserId == userId).Select(m => (int?)m.ShopId).SingleOrDefaultAsync(token);

    public static IQueryable<Product> ProductsFor(this AppDbContext db, int userId) =>
        db.Products.Where(p => db.Set<ProductShop>().Any(link => link.ProductId == p.Id &&
            db.Set<ShopMembership>().Any(m => m.UserId == userId && m.ShopId == link.ShopId)));

    public static IQueryable<Order> OrdersFor(this AppDbContext db, int userId) =>
        db.Orders.Where(o => db.Set<OrderShop>().Any(link => link.OrderId == o.Id &&
            db.Set<ShopMembership>().Any(m => m.UserId == userId && m.ShopId == link.ShopId)));
}
