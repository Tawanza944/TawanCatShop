using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Converters;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /// <summary>
    /// คลาสบริบทฐานข้อมูล (Entity Framework Core Database Context)
    /// ทำหน้าที่เป็นสะพานเชื่อมต่อและจัดการข้อมูลระหว่างระบบ ASP.NET Core และฐานข้อมูล MySQL (csi402db)
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// ตารางข้อมูลผู้ใช้งานระบบ (ตาราง users)
        /// </summary>
        public void RecordAdminActivity(string actor, string description) =>
            AdminActivities.Add(new AdminActivity { Actor = actor, Description = description, CreatedUtc = DateTime.UtcNow });

        public DbSet<AdminActivity> AdminActivities => Set<AdminActivity>();

        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// ตารางข้อมูลบทบาทหรือสิทธิ์การใช้งาน (ตาราง roles: Admin, User, Raider, Owner, Accounting, Shop)
        /// </summary>
        public DbSet<Role> Roles { get; set; } = null!;

        /// <summary>
        /// ตารางเชื่อมโยงบทบาทของผู้ใช้ (ตาราง staff: จับคู่ user_id กับ role_id)
        /// </summary>
        public DbSet<Staff> Staff { get; set; } = null!;

        /// <summary>
        /// ตารางข้อมูลคำสั่งซื้อจริง (ตาราง orders)
        /// </summary>
        public DbSet<Order> Orders { get; set; } = null!;

        /// <summary>
        /// ตารางข้อมูลรายการสินค้าในคำสั่งซื้อ (ตาราง order_items)
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        /// <summary>
        /// ตารางข้อมูลสินค้าจริงในร้านค้า (ตาราง products)
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// ตารางบันทึกการใช้งานโค้ดส่วนลดของสมาชิก (ตาราง user_coupons: จำกัด 1 โค้ด ต่อ 1 บัญชีผู้ใช้)
        /// </summary>
        public DbSet<UserCoupon> UserCoupons { get; set; } = null!;

        /// <summary>
        /// ตารางข้อมูลคูปองและโปรโมชั่นส่วนลด (ตาราง coupons: Admin สามารถเพิ่ม/แก้ไข/ลบ/เปิด-ปิดได้)
        /// </summary>
        public DbSet<Coupon> Coupons { get; set; } = null!;

        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

        /// <summary>
        /// เมธอดกำหนดค่าการแมปความสัมพันธ์และคอลัมน์ของตารางในฐานข้อมูล MySQL (Fluent API)
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("product_reviews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("user_addresses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(50);
                entity.Property(e => e.RecipientName).HasColumnName("recipient_name").HasMaxLength(100);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.AddressLine).HasColumnName("address_line");
                entity.Property(e => e.IsDefault).HasColumnName("is_default");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            MarketplaceMapping.Configure(modelBuilder);
            ShippingMapping.Configure(modelBuilder);

            modelBuilder.Entity<SavedAddressRegion>(entity =>
            {
                entity.ToTable("user_address_regions");
                entity.HasKey(region => region.AddressId);
                entity.Property(region => region.AddressId).HasColumnName("address_id").ValueGeneratedNever();
                entity.Property(region => region.ProvinceCode).HasColumnName("province_code").HasMaxLength(2);
                entity.Property(region => region.DistrictCode).HasColumnName("district_code").HasMaxLength(4);
                entity.Property(region => region.SubdistrictCode).HasColumnName("subdistrict_code").HasMaxLength(6);
                entity.Property(region => region.PostalCode).HasColumnName("postal_code").HasMaxLength(5);
                entity.HasOne<UserAddress>().WithOne(address => address.Region)
                    .HasForeignKey<SavedAddressRegion>(region => region.AddressId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LedgerEntry>(entity =>
            {
                entity.ToTable("ledger_entries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EntryDate).HasColumnName("entry_date").HasColumnType("date").HasConversion(new DateOnlyConverter());
                entity.Property(e => e.EntryType).HasColumnName("entry_type").HasMaxLength(10);
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnName("amount");
                entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // การแมปข้อมูลตาราง users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Ignore(e => e.Role); // ข้ามฟิลด์ Role เนื่องจากไม่ได้อยู่ในตาราง users โดยตรง (ดึงผ่าน staff)
            });

            // การแมปข้อมูลตาราง roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            // การแมปข้อมูลตาราง staff (เชื่อมโยง User และ Role)
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("staff");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // การแมปข้อมูลตาราง orders (คำสั่งซื้อสินค้า)
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderNo).HasColumnName("order_no");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CustomerName).HasColumnName("customer_name");
                entity.Property(e => e.CustomerPhone).HasColumnName("customer_phone");
                entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address");
                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
                entity.Property(e => e.Subtotal).HasColumnName("subtotal");
                entity.Property(e => e.Discount).HasColumnName("discount");
                entity.Property(e => e.ShippingFee).HasColumnName("shipping_fee");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
                entity.Property(e => e.CouponCode).HasColumnName("coupon_code");
                entity.Property(e => e.CommissionDeducted).HasColumnName("commission_deducted");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasConversion(new OrderStatusConverter());
                entity.Property(e => e.TrackingNote).HasColumnName("tracking_note");
                entity.Property(e => e.RiderId).HasColumnName("rider_id");
                entity.Property(e => e.PickedUpAt).HasColumnName("picked_up_at");
                entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
                entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasMany(e => e.Items)
                      .WithOne(i => i.Order)
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShopPayout>(entity =>
            {
                entity.ToTable("shop_payouts");
                entity.HasKey(p => p.OrderId);
                entity.Property(p => p.OrderId).HasColumnName("order_id").ValueGeneratedNever();
                entity.Property(p => p.Commission).HasColumnName("commission").HasPrecision(12, 2);
                entity.Property(p => p.NetAmount).HasColumnName("net_amount").HasPrecision(12, 2);
                entity.Property(p => p.ApprovedBy).HasColumnName("approved_by");
                entity.Property(p => p.ApprovedAt).HasColumnName("approved_at");
                entity.Property(p => p.RecordedBy).HasColumnName("recorded_by");
                entity.Property(p => p.RecordedAt).HasColumnName("recorded_at");
                entity.Property(p => p.TransferDate).HasColumnName("transfer_date")
                    .HasColumnType("date").HasConversion(new DateOnlyConverter());
                entity.Property(p => p.TransferReference).HasColumnName("transfer_reference").HasMaxLength(100);
                entity.HasOne(p => p.Order).WithOne(o => o.ShopPayout)
                    .HasForeignKey<ShopPayout>(p => p.OrderId).OnDelete(DeleteBehavior.Restrict);
            });

            // การแมปข้อมูลตาราง order_items (รายการสินค้าในแต่ละคำสั่งซื้อ)
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.ProductName).HasColumnName("product_name");
                entity.Property(e => e.ProductImage).HasColumnName("product_image");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            });

            // การแมปข้อมูลตาราง products (สินค้าในร้านค้า LoveCat)
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Category).HasColumnName("category");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.OriginalPrice).HasColumnName("original_price");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.Icon).HasColumnName("icon");
                entity.Property(e => e.Badge).HasColumnName("badge");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.ReviewCount).HasColumnName("review_count");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            // การแมปข้อมูลตาราง user_coupons (บันทึกการใช้โค้ดส่วนลดของสมาชิก)
            modelBuilder.Entity<UserCoupon>(entity =>
            {
                entity.ToTable("user_coupons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CouponCode).HasColumnName("coupon_code");
                entity.Property(e => e.OrderNo).HasColumnName("order_no");
                entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount");
                entity.Property(e => e.UsedAt).HasColumnName("used_at");
            });

            // การแมปข้อมูลตาราง coupons (ข้อมูลโค้ดโปรโมชั่นและส่วนลด)
            modelBuilder.Entity<SavedCoupon>(entity =>
            {
                entity.ToTable("saved_coupons");
                entity.HasKey(row => new { row.UserId, row.CouponId });
                entity.Property(row => row.UserId).HasColumnName("user_id");
                entity.Property(row => row.CouponId).HasColumnName("coupon_id");
                entity.Property(row => row.SavedAt).HasColumnName("saved_at");
                entity.HasOne<User>().WithMany().HasForeignKey(row => row.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Coupon>().WithMany().HasForeignKey(row => row.CouponId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.ToTable("coupons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Code).HasColumnName("code");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.DiscountType).HasColumnName("discount_type");
                entity.Property(e => e.DiscountValue).HasColumnName("discount_value");
                entity.Property(e => e.MinSpend).HasColumnName("min_spend");
                entity.Property(e => e.MaxDiscount).HasColumnName("max_discount");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.ApplicableCategories).HasColumnName("applicable_categories");
                entity.Ignore(e => e.ApplicableCategoryList);
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
        }
    }
}
