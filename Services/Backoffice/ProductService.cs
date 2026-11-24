using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Common;
using WebApplication1.Models;
using WebApplication1.Services.Storage;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

public sealed class ProductService : IProductService
{
    private const int MaxRestock = 1000;

    private readonly AppDbContext _db;
    private readonly IImageStorage _images;
    private readonly TimeProvider _clock;
    private readonly ICurrentUserService _actor;

    public ProductService(AppDbContext db, IImageStorage images, TimeProvider clock, ICurrentUserService actor)
    {
        _db = db;
        _images = images;
        _clock = clock;
        _actor = actor;
    }

    public async Task<OperationResult<int>> SaveAsync(ProductForm form, CancellationToken cancellationToken = default)
    {
        if (StorefrontCategories.FindByName(form.Category) is null)
        {
            return OperationResult<int>.Failure("หมวดสินค้าไม่ถูกต้อง");
        }

        if (form.OriginalPrice is > 0 && form.OriginalPrice <= form.Price)
        {
            return OperationResult<int>.Failure("ราคาก่อนลดต้องมากกว่าราคาขาย (เว้นว่างถ้าไม่ได้ลดราคา)");
        }

        var userId = _actor.RequireUserId();
        var shopId = await _db.ShopIdAsync(userId, cancellationToken);
        if (shopId is null) return OperationResult<int>.Failure("บัญชีนี้ยังไม่ได้ผูกกับร้านค้า");

        Product product;
        if (form.Id == 0)
        {
            product = new Product { CreatedAt = _clock.GetUtcNow().UtcDateTime, Rating = 0, ReviewCount = 0 };
            product.Shop = new ProductShop { ShopId = shopId.Value };
            _db.Products.Add(product);
        }
        else
        {
            var existing = await _db.ProductsFor(userId).FirstOrDefaultAsync(p => p.Id == form.Id, cancellationToken);
            if (existing is null)
            {
                return OperationResult<int>.Failure("ไม่พบสินค้านี้");
            }

            product = existing;
            product.UpdatedAt = _clock.GetUtcNow().UtcDateTime;
        }

        if (form.ImageFile is { Length: > 0 })
        {
            var upload = await _images.SaveAsync(form.ImageFile, "products", cancellationToken);
            if (!upload.Succeeded)
            {
                return OperationResult<int>.Failure(upload.Error!);
            }

            _images.Delete(product.ImageUrl);
            product.ImageUrl = upload.Value;
        }
        else if (!string.IsNullOrWhiteSpace(form.ImageUrl))
        {
            product.ImageUrl = form.ImageUrl.Trim();
        }

        product.Name = form.Name.Trim();
        product.Category = form.Category;
        product.Price = form.Price;
        product.OriginalPrice = form.OriginalPrice is > 0 ? form.OriginalPrice : null;
        product.Stock = form.Stock;
        product.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Success(product.Id);
    }

    public async Task<OperationResult<int>> RestockAsync(int productId, int amount, CancellationToken cancellationToken = default)
    {
        if (amount is < 1 or > MaxRestock)
        {
            return OperationResult<int>.Failure($"เติมได้ครั้งละ 1–{MaxRestock} ชิ้น");
        }

        var now = _clock.GetUtcNow().UtcDateTime;
        var changed = await _db.ProductsFor(_actor.RequireUserId()).Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + amount).SetProperty(p => p.UpdatedAt, now), cancellationToken);
        if (changed == 0)
        {
            return OperationResult<int>.Failure("ไม่พบสินค้านี้");
        }

        var stock = await _db.ProductsFor(_actor.RequireUserId()).Where(p => p.Id == productId).Select(p => p.Stock).SingleAsync(cancellationToken);
        return OperationResult<int>.Success(stock);
    }

    public async Task<OperationResult<bool>> DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.ProductsFor(_actor.RequireUserId()).FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return OperationResult.Fail("ไม่พบสินค้านี้");
        }

        // ประวัติออเดอร์เก็บสำเนาชื่อ ราคา และตำแหน่งรูปสินค้าไว้เพื่อให้ยังแสดงได้หลังลบสินค้า
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        var stillUsedByOrders = await _db.OrderItems.AnyAsync(i => i.ProductImage == product.ImageUrl, cancellationToken);
        if (!stillUsedByOrders)
        {
            _images.Delete(product.ImageUrl);
        }

        return OperationResult.Ok();
    }
}
