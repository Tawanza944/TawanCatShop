using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

internal static class MarketplaceMapping
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<FlashReservation>(e =>
        {
            e.ToTable("flash_reservations");
            e.HasKey(r => r.OrderItemId);
            e.Property(r => r.OrderItemId).ValueGeneratedNever();
            e.HasOne<OrderItem>().WithOne().HasForeignKey<FlashReservation>(r => r.OrderItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<FlashSale>().WithMany().HasForeignKey(r => r.FlashSaleId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<FlashSale>(e =>
        {
            e.ToTable("flash_sales");
            e.Property(p => p.Price).HasPrecision(12, 2);
            e.HasIndex(p => new { p.ProductId, p.StartsAt, p.EndsAt });
            e.HasOne<Product>().WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<OrderIncentives>(e =>
        {
            e.ToTable("order_incentives");
            e.HasKey(p => p.OrderId);
            e.Property(p => p.OrderId).ValueGeneratedNever();
            e.Property(p => p.FlashDiscount).HasPrecision(12, 2);
            e.Property(p => p.CouponDiscount).HasPrecision(12, 2);
            e.Property(p => p.RewardUsed).HasPrecision(12, 2);
            e.Property(p => p.EligibleSpend).HasPrecision(12, 2);
            e.HasOne<Order>().WithOne().HasForeignKey<OrderIncentives>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<ShopReviewReply>(e =>
        {
            e.ToTable("shop_review_replies");
            e.HasKey(p => p.ReviewId);
            e.Property(p => p.ReviewId).ValueGeneratedNever();
            e.Property(p => p.Message).HasMaxLength(1000);
            e.HasOne<ProductReview>().WithOne().HasForeignKey<ShopReviewReply>(p => p.ReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<MarketplaceShop>().WithMany().HasForeignKey(p => p.ShopId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<ShopFollow>(e =>
        {
            e.ToTable("shop_follows");
            e.HasKey(p => new { p.ShopId, p.UserId });
            e.HasOne<MarketplaceShop>().WithMany().HasForeignKey(p => p.ShopId);
            e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId);
        });
        model.Entity<ShopMessage>(e =>
        {
            e.ToTable("shop_messages");
            e.Property(p => p.Message).HasMaxLength(1000);
            e.HasIndex(p => new { p.ShopId, p.UserId, p.Id });
            e.HasOne<MarketplaceShop>().WithMany().HasForeignKey(p => p.ShopId);
            e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId);
        });
        model.Entity<MarketplaceShop>(e =>
        {
            e.ToTable("marketplace_shops");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(p => p.PickupAddress).HasColumnName("pickup_address").HasMaxLength(500);
            e.HasIndex(p => p.Name).IsUnique();
        });
        model.Entity<ShopProfileImage>(e =>
        {
            e.ToTable("shop_profile_images");
            e.HasKey(p => p.ShopId);
            e.Property(p => p.ShopId).HasColumnName("shop_id").ValueGeneratedNever();
            e.Property(p => p.ImageUrl).HasColumnName("image_url").HasMaxLength(255).IsConcurrencyToken();
            e.HasOne<MarketplaceShop>().WithOne().HasForeignKey<ShopProfileImage>(p => p.ShopId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<ShopMembership>(e =>
        {
            e.ToTable("shop_memberships");
            e.HasKey(p => p.UserId);
            e.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever();
            e.Property(p => p.ShopId).HasColumnName("shop_id");
            e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<MarketplaceShop>().WithMany().HasForeignKey(p => p.ShopId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<RiderProvince>(e =>
        {
            e.ToTable("rider_provinces");
            e.HasKey(p => p.UserId);
            e.Property(p => p.UserId).HasColumnName("user_id").ValueGeneratedNever();
            e.Property(p => p.ProvinceCode).HasColumnName("province_code").HasMaxLength(2);
            e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<ProductShop>(e =>
        {
            e.ToTable("product_shops");
            e.HasKey(p => p.ProductId);
            e.Property(p => p.ProductId).HasColumnName("product_id").ValueGeneratedNever();
            e.Property(p => p.ShopId).HasColumnName("shop_id");
            e.HasOne<Product>().WithOne(p => p.Shop).HasForeignKey<ProductShop>(p => p.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Shop).WithMany().HasForeignKey(p => p.ShopId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<BasketPayment>(e =>
        {
            e.ToTable("basket_payments");
            e.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.Total).HasColumnName("total").HasPrecision(12, 2);
            e.Property(p => p.CouponDiscount).HasColumnName("coupon_discount").HasPrecision(12, 2);
            e.Property(p => p.ShippingFee).HasColumnName("shipping_fee").HasPrecision(12, 2);
            e.Property(p => p.CouponCode).HasColumnName("coupon_code").HasMaxLength(50);
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.Status).HasColumnName("status").HasMaxLength(12);
            e.Property(p => p.ReviewedBy).HasColumnName("reviewed_by");
            e.Property(p => p.ReviewedAt).HasColumnName("reviewed_at");
            e.Property(p => p.Reason).HasColumnName("reason").HasMaxLength(300);
            e.HasIndex(p => new { p.UserId, p.CreatedAt });
        });
        model.Entity<OrderShop>(e =>
        {
            e.ToTable("order_shops");
            e.HasKey(p => p.OrderId);
            e.Property(p => p.OrderId).HasColumnName("order_id").ValueGeneratedNever();
            e.Property(p => p.ShopId).HasColumnName("shop_id");
            e.Property(p => p.PaymentId).HasColumnName("payment_id").HasMaxLength(36);
            e.Property(p => p.ProvinceCode).HasColumnName("province_code").HasMaxLength(2);
            e.Property(p => p.RiderDelivery).HasColumnName("rider_delivery");
            e.HasOne<Order>().WithOne(p => p.Marketplace).HasForeignKey<OrderShop>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Shop).WithMany().HasForeignKey(p => p.ShopId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Payment).WithMany().HasForeignKey(p => p.PaymentId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<BankSlip>(e =>
        {
            e.ToTable("bank_slips");
            e.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            e.Property(p => p.PaymentId).HasColumnName("payment_id").HasMaxLength(36);
            e.Property(p => p.PayoutOrderId).HasColumnName("payout_order_id");
            e.Property(p => p.Checksum).HasColumnName("checksum").HasMaxLength(64);
            e.HasIndex(p => p.Checksum).IsUnique();
            e.Property(p => p.ContentType).HasColumnName("content_type").HasMaxLength(20);
            e.Property(p => p.Content).HasColumnName("content").HasColumnType("mediumblob");
            e.HasOne<BasketPayment>().WithOne(p => p.Slip).HasForeignKey<BankSlip>(p => p.PaymentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ShopPayout>().WithOne().HasForeignKey<BankSlip>(p => p.PayoutOrderId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
