using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Identity;
using WebApplication1.Domain.Orders;
using WebApplication1.Services.Identity;
using WebApplication1.Models;

namespace WebApplication1.Services.Orders;

public sealed class OrderFulfillmentService : IOrderFulfillmentService
{
    private readonly AppDbContext _db;
    private readonly IOrderWorkflow _workflow;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _clock;
    private readonly ILogger<OrderFulfillmentService> _logger;

    public OrderFulfillmentService(AppDbContext db, IOrderWorkflow workflow, ICurrentUserService currentUser, TimeProvider clock, ILogger<OrderFulfillmentService> logger)
    {
        _db = db;
        _workflow = workflow;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<OperationResult<OrderStatus>> MoveToAsync(string orderNo, OrderStatus nextStatus, CancellationToken cancellationToken = default)
    {
        if (nextStatus == OrderStatus.Cancelled)
        {
            var cancelled = await CancelAsync(orderNo, cancellationToken);
            return cancelled.Succeeded ? OperationResult<OrderStatus>.Success(OrderStatus.Cancelled) : OperationResult<OrderStatus>.Failure(cancelled.Error!);
        }

        var order = await VisibleOrders().AsNoTracking().Include(o => o.Marketplace).ThenInclude(link => link!.Payment).FirstOrDefaultAsync(o => o.OrderNo == orderNo, cancellationToken);
        if (order is null)
        {
            return OperationResult<OrderStatus>.Failure("ไม่พบคำสั่งซื้อนี้");
        }

        var customerConfirmation = nextStatus == OrderStatus.Delivered && _currentUser.IsInRole(Roles.User) &&
            order.UserId == _currentUser.UserId && order.Status == OrderStatus.Delivering && order.Marketplace is { RiderDelivery: false };
        if (order.Marketplace?.Payment is { Status: not PaymentReview.Confirmed })
            return OperationResult<OrderStatus>.Failure("ต้องตรวจสลิปผ่านก่อนจัดส่ง");

        if (nextStatus == OrderStatus.Ready && order.Marketplace is { RiderDelivery: false })
            return OperationResult<OrderStatus>.Failure("ออเดอร์นี้ใช้การส่งพัสดุ");

        // ออเดอร์พัสดุเริ่มจัดส่งผ่าน IShipmentService เพื่อให้มีชื่อขนส่งและเลขติดตามก่อนเปลี่ยนสถานะ
        if (nextStatus == OrderStatus.Delivering && order.Marketplace is { RiderDelivery: false })
            return OperationResult<OrderStatus>.Failure("ออเดอร์ส่งพัสดุต้องเลือกขนส่งและกรอกเลขพัสดุก่อน");

        if (!customerConfirmation && !_workflow.CanTransition(order.Status, nextStatus, _currentUser.Role))
        {
            return OperationResult<OrderStatus>.Failure($"เปลี่ยนจาก \"{order.Status.ToThai()}\" เป็น \"{nextStatus.ToThai()}\" ไม่ได้");
        }

        var riderId = _currentUser.RequireUserId();
        if (nextStatus == OrderStatus.Delivered && !customerConfirmation && order.RiderId != riderId)
        {
            return OperationResult<OrderStatus>.Failure("งานนี้มีไรเดอร์คนอื่นรับไปแล้ว");
        }

        if (nextStatus == OrderStatus.Delivering)
        {
            var province = await _db.Set<RiderProvince>().Where(p => p.UserId == riderId)
                .Select(p => p.ProvinceCode).SingleOrDefaultAsync(cancellationToken);
            if (order.Marketplace is not { RiderDelivery: true } delivery || province is null || province != delivery.ProvinceCode)
                return OperationResult<OrderStatus>.Failure("รับงานได้เฉพาะในจังหวัดของคุณ");
        }

        var now = _clock.GetUtcNow().UtcDateTime;
        var note = _workflow.TrackingNoteFor(nextStatus);
        var previous = order.Status;

        // ใช้สถานะเดิมเป็นเงื่อนไข เพื่อปฏิเสธคำขอเมื่อมีคนเปลี่ยนสถานะออเดอร์ไปก่อนแล้ว
        var query = VisibleOrders().Where(o => o.Id == order.Id && o.Status == previous);
        var changed = nextStatus switch
        {
            OrderStatus.Delivering => await query.Where(o => o.RiderId == null && _db.Set<OrderShop>().Any(link => link.OrderId == o.Id &&
                _db.Set<RiderProvince>().Any(p => p.UserId == riderId && p.ProvinceCode == link.ProvinceCode))).ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, nextStatus).SetProperty(o => o.TrackingNote, note)
                .SetProperty(o => o.RiderId, riderId).SetProperty(o => o.PickedUpAt, now).SetProperty(o => o.UpdatedAt, now), cancellationToken),
            OrderStatus.Delivered => await query.Where(o => customerConfirmation ? o.UserId == riderId : o.RiderId == riderId).ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, nextStatus).SetProperty(o => o.TrackingNote, note)
                .SetProperty(o => o.DeliveredAt, now).SetProperty(o => o.UpdatedAt, now), cancellationToken),
            _ => await query.ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, nextStatus).SetProperty(o => o.TrackingNote, note).SetProperty(o => o.UpdatedAt, now), cancellationToken)
        };

        if (changed == 0)
        {
            return OperationResult<OrderStatus>.Failure("คำสั่งซื้อนี้ถูกเปลี่ยนสถานะไปแล้ว กรุณาโหลดหน้าใหม่");
        }

        _logger.LogInformation("Order {OrderNo}: {From} -> {To} by user {UserId}.", orderNo, previous, nextStatus, riderId);
        return OperationResult<OrderStatus>.Success(nextStatus);
    }

    public async Task<OperationResult<bool>> CancelAsync(string orderNo, CancellationToken cancellationToken = default)
    {
        var order = await VisibleOrders().AsNoTracking().Include(o => o.Items).Include(o => o.Marketplace).ThenInclude(link => link!.Payment)
            .FirstOrDefaultAsync(o => o.OrderNo == orderNo, cancellationToken);
        if (order is null)
        {
            return OperationResult.Fail("ไม่พบคำสั่งซื้อนี้");
        }

        var userId = _currentUser.RequireUserId();
        if (_currentUser.IsInRole(Roles.User) && order.UserId != userId)
        {
            return OperationResult.Fail("ยกเลิกได้เฉพาะคำสั่งซื้อของคุณเอง");
        }

        if (order.Marketplace?.Payment is not null)
            return OperationResult.Fail("คำสั่งซื้อที่มีการชำระรวม ให้ติดต่อฝ่ายบัญชีเพื่อจัดการยอดชำระ");

        if (!_workflow.CanTransition(order.Status, OrderStatus.Cancelled, _currentUser.Role))
        {
            return OperationResult.Fail($"คำสั่งซื้อที่ \"{order.Status.ToThai()}\" ยกเลิกไม่ได้แล้ว");
        }

        var now = _clock.GetUtcNow().UtcDateTime;
        var note = _workflow.TrackingNoteFor(OrderStatus.Cancelled);
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var changed = await _db.Orders.Where(o => o.Id == order.Id && o.Status == order.Status)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status, OrderStatus.Cancelled).SetProperty(o => o.TrackingNote, note)
                .SetProperty(o => o.CancelledAt, now).SetProperty(o => o.UpdatedAt, now), cancellationToken);
        if (changed == 0)
        {
            return OperationResult.Fail("คำสั่งซื้อนี้ถูกเปลี่ยนสถานะไปแล้ว กรุณาโหลดหน้าใหม่");
        }

        foreach (var item in order.Items)
        {
            await _db.Products.Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity).SetProperty(p => p.UpdatedAt, now), cancellationToken);
        }

        await _db.UserCoupons.Where(c => c.OrderNo == order.OrderNo).ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNo} cancelled by user {UserId}.", orderNo, userId);
        return OperationResult.Ok();
    }
    private IQueryable<Order> VisibleOrders() => _currentUser.IsInRole(Roles.Shop)
        ? _db.OrdersFor(_currentUser.RequireUserId())
        : _currentUser.IsInRole(Roles.User)
            ? _db.Orders.Where(order => order.UserId == _currentUser.RequireUserId())
            : _db.Orders;
}
