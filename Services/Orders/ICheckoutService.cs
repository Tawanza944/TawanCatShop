using WebApplication1.Domain.Common;
using WebApplication1.ViewModels.Checkout;

namespace WebApplication1.Services.Orders;

public sealed record PlacedOrder(string OrderNo, decimal Total);

/// <summary>ตรวจราคาฝั่งเซิร์ฟเวอร์ จองสต็อก สร้างออเดอร์ และบันทึกคูปองในธุรกรรมเดียวกัน</summary>
public interface ICheckoutService
{
    Task<OperationResult<PlacedOrder>> PlaceOrderAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken = default);
}
