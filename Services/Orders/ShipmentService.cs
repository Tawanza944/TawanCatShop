using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Domain.Shipping;
using WebApplication1.Models;
using WebApplication1.Services.Identity;

namespace WebApplication1.Services.Orders;

/// <inheritdoc cref="IShipmentService"/>
public sealed class ShipmentService : IShipmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _clock;
    private readonly ILogger<ShipmentService> _logger;

    public ShipmentService(AppDbContext db, ICurrentUserService currentUser, TimeProvider clock, ILogger<ShipmentService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<OperationResult<bool>> DispatchAsync(string orderNo, ShipmentDetails shipment, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsInRole(Roles.Shop))
            return OperationResult.Fail("เฉพาะร้านค้าที่ส่งพัสดุได้");

        var shopUserId = _currentUser.RequireUserId();

        // จำกัดการค้นหาตามร้านที่ล็อกอิน ออเดอร์ของร้านอื่นจะแสดงผลเหมือนไม่พบออเดอร์
        var order = await _db.OrdersFor(shopUserId).AsNoTracking()
            .Include(o => o.Marketplace).ThenInclude(link => link!.Payment)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderNo == orderNo, cancellationToken);

        var refusal = Validate(order);
        if (refusal is not null)
            return OperationResult.Fail(refusal);

        if (await TrackingNumberTakenAsync(shipment, cancellationToken))
            return OperationResult.Fail("เลขพัสดุนี้ถูกใช้กับออเดอร์อื่นแล้ว ตรวจเลขอีกครั้ง");

        var now = _clock.GetUtcNow().UtcDateTime;
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.Add(new OrderShipment
            {
                OrderId = order!.Id,
                Carrier = shipment.Carrier,
                OtherCarrierName = shipment.OtherCarrierName,
                TrackingNumber = shipment.TrackingNumber,
                ShippedBy = shopUserId,
                ShippedAt = now
            });
            await _db.SaveChangesAsync(cancellationToken);

            // เปลี่ยนสถานะได้ต่อเมื่อออเดอร์ยังอยู่ระหว่างแพ็ก ณ เวลาที่อัปเดต
            var note = $"ส่งพัสดุทาง {shipment.CarrierDisplayName} เลขพัสดุ {shipment.TrackingNumber}";
            var changed = await _db.Orders
                .Where(o => o.Id == order.Id && o.Status == OrderStatus.Packing)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.Status, OrderStatus.Delivering)
                    .SetProperty(o => o.TrackingNote, note)
                    .SetProperty(o => o.UpdatedAt, now), cancellationToken);
            if (changed == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult.Fail("คำสั่งซื้อนี้ถูกเปลี่ยนสถานะไปแล้ว กรุณาโหลดหน้าใหม่");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // รองรับกรณีอีกคำขอบันทึกออเดอร์หรือเลขพัสดุเดียวกันก่อนการเพิ่มข้อมูลครั้งนี้
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(exception, "Shipment for order {OrderNo} was rejected by a database constraint.", orderNo);
            return OperationResult.Fail("ออเดอร์นี้ส่งพัสดุแล้ว หรือเลขพัสดุถูกใช้ไปแล้ว กรุณาโหลดหน้าใหม่");
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        _logger.LogInformation("Order {OrderNo} shipped by user {UserId} via {Carrier} {Tracking}.",
            orderNo, shopUserId, shipment.Carrier.ToCode(), shipment.TrackingNumber);
        return OperationResult.Ok();
    }

    /// <summary>ตรวจเงื่อนไขตามลำดับที่ร้านแก้ได้ คืนข้อความภาษาไทยเมื่อมีข้อผิดพลาด</summary>
    private static string? Validate(Order? order) => order switch
    {
        null => "ไม่พบคำสั่งซื้อนี้ในร้านของคุณ",
        { Marketplace: null } or { Marketplace.RiderDelivery: true } => "ออเดอร์นี้ส่งโดยไรเดอร์ ไม่ต้องกรอกเลขพัสดุ",
        { Shipment: not null } => "ออเดอร์นี้ส่งพัสดุแล้ว แก้ไขเลขพัสดุไม่ได้",
        { Status: OrderStatus.Cancelled } => "ออเดอร์นี้ถูกยกเลิกแล้ว",
        { Marketplace.Payment.Status: not PaymentReview.Confirmed } => "ยังตรวจสลิปไม่ผ่าน ส่งพัสดุไม่ได้",
        { Status: not OrderStatus.Packing } => "ส่งพัสดุได้เฉพาะออเดอร์ที่แพ็กแล้ว",
        _ => null
    };

    private Task<bool> TrackingNumberTakenAsync(ShipmentDetails shipment, CancellationToken cancellationToken) =>
        _db.Set<OrderShipment>().AnyAsync(s => s.Carrier == shipment.Carrier && s.TrackingNumber == shipment.TrackingNumber, cancellationToken);
}
