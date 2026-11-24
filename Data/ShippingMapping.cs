using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Converters;
using WebApplication1.Domain.Shipping;
using WebApplication1.Models;

namespace WebApplication1.Data;

/// <summary>การแมปตารางจัดส่งพัสดุใน EF Core ซึ่งสร้างด้วย database/12_order_shipments.sql</summary>
internal static class ShippingMapping
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<OrderShipment>(e =>
        {
            e.ToTable("order_shipments");
            e.HasKey(s => s.OrderId);
            e.Property(s => s.OrderId).HasColumnName("order_id").ValueGeneratedNever();
            e.Property(s => s.Carrier).HasColumnName("carrier").HasMaxLength(30).HasConversion(new CarrierConverter());
            e.Property(s => s.OtherCarrierName).HasColumnName("carrier_name").HasMaxLength(ShipmentDetails.CarrierNameMaxLength);
            e.Property(s => s.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(ShipmentDetails.TrackingMaxLength);
            e.Property(s => s.ShippedBy).HasColumnName("shipped_by");
            e.Property(s => s.ShippedAt).HasColumnName("shipped_at");
            e.Ignore(s => s.Details);
            e.HasIndex(s => new { s.Carrier, s.TrackingNumber }).IsUnique().HasDatabaseName("uq_order_shipments_tracking");
            e.HasOne<Order>().WithOne(o => o.Shipment).HasForeignKey<OrderShipment>(s => s.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(s => s.ShippedBy).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
