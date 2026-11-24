using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Addresses;
using WebApplication1.Domain.Orders;
using WebApplication1.Domain.Pricing;
using WebApplication1.Models;
using WebApplication1.ViewModels.Checkout;

namespace WebApplication1.Services.Orders;

public sealed class CheckoutService : ICheckoutService
{
    private const int MaxOrderNumberAttempts = 3;

    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    private readonly IOrderNumberGenerator _orderNumbers;
    private readonly IOrderWorkflow _workflow;
    private readonly TimeProvider _clock;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        AppDbContext db,
        IPricingService pricing,
        IOrderNumberGenerator orderNumbers,
        IOrderWorkflow workflow,
        TimeProvider clock,
        ILogger<CheckoutService> logger)
    {
        _db = db;
        _pricing = pricing;
        _orderNumbers = orderNumbers;
        _workflow = workflow;
        _clock = clock;
        _logger = logger;
    }

    public async Task<OperationResult<PlacedOrder>> PlaceOrderAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (!PaymentMethods.TryParseCode(request.PaymentMethod, out var paymentMethod) || paymentMethod != PaymentMethod.PromptPay)
        {
            return OperationResult<PlacedOrder>.Failure("วิธีชำระเงินนี้ร้านยังไม่รองรับ");
        }

        if (request.RequestId == Guid.Empty)
            return OperationResult<PlacedOrder>.Failure("กรุณาเปิดหน้าชำระเงินอีกครั้ง");

        var existing = await FindPaymentAsync(userId, request.RequestId, cancellationToken);
        if (existing is not null) return existing;

        var region = ThailandAddressCatalog.Format(request.Region);
        if (region is null)
            return OperationResult<PlacedOrder>.Failure("เลือกจังหวัด อำเภอ ตำบล และรหัสไปรษณีย์ให้ตรงกัน");

        var quote = await _pricing.QuoteAsync(request.Items.Select(i => i.ToRequest()).ToList(), request.CouponCode, userId, cancellationToken, request.UseRewards);
        if (!quote.CanCheckout)
        {
            return OperationResult<PlacedOrder>.Failure(quote.Problems.FirstOrDefault() ?? "ตะกร้าว่าง");
        }

        if (!string.IsNullOrWhiteSpace(request.CouponCode) && quote.CouponError is not null)
        {
            return OperationResult<PlacedOrder>.Failure(quote.CouponError);
        }

        if (request.ExpectedTotal != quote.Total)
            return OperationResult<PlacedOrder>.Failure("ยอดตะกร้าเปลี่ยน กรุณาตรวจยอดก่อนส่งสลิป");

        var slip = SlipImage.Decode(request.Slip);
        if (quote.Total > 0 && slip is null)
            return OperationResult<PlacedOrder>.Failure("แนบสลิป JPG หรือ PNG ขนาดไม่เกิน 2 MB");

        for (var attempt = 1; attempt <= MaxOrderNumberAttempts; attempt++)
        {
            try
            {
                var saved = await SaveOrderAsync(userId, request, paymentMethod, quote, region, slip, cancellationToken);
                if (saved.Succeeded) return saved;
                existing = await FindPaymentAsync(userId, request.RequestId, cancellationToken);
                return existing ?? saved;
            }
            catch (DbUpdateException ex) when (IsDuplicateOrderNumber(ex))
            {
                _logger.LogWarning("Order number collision on attempt {Attempt}; retrying.", attempt);
                _db.ChangeTracker.Clear();
                existing = await FindPaymentAsync(userId, request.RequestId, cancellationToken);
                if (existing is not null) return existing;
                if (slip is not null && await _db.Set<BankSlip>().AnyAsync(evidence => evidence.Checksum == slip.Checksum, cancellationToken))
                    return OperationResult<PlacedOrder>.Failure("สลิปนี้ถูกบันทึกแล้ว");
            }
        }

        return OperationResult<PlacedOrder>.Failure("ระบบไม่ว่าง กรุณากดสั่งซื้ออีกครั้ง");
    }

    private async Task<OperationResult<PlacedOrder>> SaveOrderAsync(
        int userId, PlaceOrderRequest request, PaymentMethod paymentMethod, PricingQuote quote, string region, BankSlip? slip, CancellationToken cancellationToken)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // ล็อกแถวลูกค้าในธุรกรรมเพื่อให้คำขอของบัญชีเดียวกันตรวจคูปองและเงินสะสมตามลำดับ
        await _db.Users.Where(user => user.Id == userId)
            .ExecuteUpdateAsync(update => update.SetProperty(user => user.Name, user => user.Name), cancellationToken);
        if (quote.CouponCode is { } code && await PricingService.IsFirstOrderCouponUnavailableAsync(_db, code, userId, cancellationToken))
            return OperationResult<PlacedOrder>.Failure(PricingService.FirstOrderCouponError);
        if (quote.CouponCode is not null && await _db.UserCoupons.AnyAsync(
            coupon => coupon.UserId == userId && coupon.CouponCode == quote.CouponCode, cancellationToken))
            return OperationResult<PlacedOrder>.Failure("คุณใช้โค้ดนี้ไปแล้ว");
        if (quote.RewardDiscount > 0 && (await _db.LoyaltyBalanceAsync(userId, cancellationToken)).Available < quote.RewardDiscount)
            return OperationResult<PlacedOrder>.Failure("ยอดเงินสะสมเปลี่ยน กรุณาตรวจยอดอีกครั้ง");

        // ตรวจสต็อก ราคา และร้านพร้อมกับตัดสต็อกในคำสั่งเดียว เพื่อกันขายเกินเมื่อมีหลายคำขอ
        foreach (var line in quote.Lines.OrderBy(item => item.ProductId))
        {
            var reserved = await _db.Products
                .Where(p => p.Id == line.ProductId && p.Stock >= line.Quantity && p.Price == (line.SellerPrice ?? line.UnitPrice) &&
                    _db.Set<ProductShop>().Any(link => link.ProductId == p.Id && link.ShopId == line.ShopId))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Stock, p => p.Stock - line.Quantity)
                    .SetProperty(p => p.UpdatedAt, now), cancellationToken);

            if (reserved == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult<PlacedOrder>.Failure($"{line.Name} สต็อกหรือราคามีการเปลี่ยนแปลง กรุณาตรวจตะกร้าอีกครั้ง");
            }

            if (line.FlashSaleId is int saleId)
            {
                var reservedSale = await _db.ActiveFlashSales(_clock.GetUtcNow().UtcDateTime)
                    .Where(s => s.Id == saleId && s.ProductId == line.ProductId && s.Price == line.UnitPrice && s.Quota - s.Sold >= line.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Sold, p => p.Sold + line.Quantity), cancellationToken);
                if (reservedSale == 0)
                    return OperationResult<PlacedOrder>.Failure($"ราคา Flash Sale ของ {line.Name} เปลี่ยนแล้ว กรุณาตรวจยอดอีกครั้ง");
            }
        }

        var payment = new BasketPayment
        {
            Id = request.RequestId, UserId = userId, Total = quote.Total, CouponCode = quote.CouponCode,
            CouponDiscount = quote.Discount, ShippingFee = quote.ShippingFee, CreatedAt = now,
            Status = quote.Total == 0 ? PaymentReview.Confirmed : PaymentReview.Pending,
            ReviewedAt = quote.Total == 0 ? now : null, Slip = slip
        };
        _db.Add(payment);
        string? firstOrderNo = null;
        var remainingCoupon = quote.Discount;
        var remainingReward = quote.RewardDiscount;
        var remainingEligible = quote.Lines.Where(IsCouponEligible).Sum(line => line.LineTotal);
        var remainingNet = quote.Subtotal - quote.Discount;
        // หนึ่งยอดชำระรวมแตกเป็นหนึ่งออเดอร์ต่อร้าน และแบ่งส่วนลดตามยอดที่เข้าเงื่อนไข
        foreach (var group in quote.Lines.GroupBy(line => line.ShopId).OrderBy(group => group.Key))
        {
            var subtotal = group.Sum(line => line.LineTotal);
            var sellerSubtotal = group.Sum(line => line.SellerTotal);
            var couponEligible = group.Where(IsCouponEligible).Sum(line => line.LineTotal);
            var coupon = LoyaltyRules.Allocate(remainingCoupon, couponEligible, remainingEligible);
            var reward = LoyaltyRules.Allocate(remainingReward, subtotal - coupon, remainingNet);
            remainingCoupon -= coupon;
            remainingReward -= reward;
            remainingEligible -= couponEligible;
            remainingNet -= subtotal - coupon;
            var order = new Order
            {
                OrderNo = await _orderNumbers.NextAsync(cancellationToken),
                UserId = userId,
                CustomerName = request.CustomerName.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),
                ShippingAddress = $"{request.ShippingAddress.Trim()} {region}",
                PaymentMethod = paymentMethod.ToThai(),
                Subtotal = sellerSubtotal,
                Discount = sellerSubtotal - subtotal + coupon + reward,
                TotalAmount = subtotal - coupon - reward,
                Status = OrderStatus.Pending,
                TrackingNote = _workflow.TrackingNoteFor(OrderStatus.Pending),
                CreatedAt = now,
                Marketplace = new OrderShop
                {
                    ShopId = group.Key, Payment = payment, ProvinceCode = request.Region!.ProvinceCode,
                    RiderDelivery = DeliveryCoverage.UsesRider(request.Region.ProvinceCode)
                },
                Items = group.Select(line => new OrderItem
                {
                    ProductId = line.ProductId, ProductName = line.Name, ProductImage = line.ImageUrl,
                    UnitPrice = line.UnitPrice, Quantity = line.Quantity, TotalPrice = line.LineTotal
                }).ToList()
            };
            firstOrderNo ??= order.OrderNo;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);
            var saleIds = group.Where(line => line.FlashSaleId.HasValue).ToDictionary(line => line.ProductId, line => line.FlashSaleId!.Value);
            foreach (var item in order.Items.Where(item => saleIds.ContainsKey(item.ProductId)))
                _db.Add(new FlashReservation { OrderItemId = item.Id, FlashSaleId = saleIds[item.ProductId] });
            _db.Add(new OrderIncentives
            {
                OrderId = order.Id, FlashDiscount = sellerSubtotal - subtotal, CouponDiscount = coupon,
                RewardUsed = reward, EligibleSpend = order.TotalAmount
            });
        }

        if (quote.CouponCode is not null)
        {
            _db.UserCoupons.Add(new UserCoupon
            {
                UserId = userId,
                CouponCode = quote.CouponCode,
                OrderNo = firstOrderNo,
                DiscountAmount = quote.Discount + quote.ShippingWaived,
                UsedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Basket {PaymentId} placed by user {UserId} for {Total}.", payment.Id, userId, payment.Total);
        return OperationResult<PlacedOrder>.Success(new PlacedOrder(firstOrderNo!, payment.Total));

        bool IsCouponEligible(PricedLine line) =>
            quote.CouponCategories is not { Count: > 0 } categories || categories.Contains(line.Category);
    }

    private async Task<OperationResult<PlacedOrder>?> FindPaymentAsync(int userId, Guid paymentId, CancellationToken token)
    {
        var existing = await _db.Orders.AsNoTracking()
            .Where(order => order.UserId == userId && order.Marketplace!.PaymentId == paymentId)
            .OrderBy(order => order.Id)
            .Select(order => new { order.OrderNo, Payment = order.Marketplace!.Payment! })
            .FirstOrDefaultAsync(token);

        if (existing is null) return null;
        // คำขอซ้ำใช้ออเดอร์เดิม แต่ยอดที่ถูกยกเลิกต้องแจ้งให้ลูกค้าเริ่มสั่งใหม่
        if (existing.Payment.Status == PaymentReview.Cancelled)
            return OperationResult<PlacedOrder>.Failure("ออเดอร์นี้ถูกยกเลิกแล้ว กรุณาเริ่มสั่งซื้อใหม่");

        return OperationResult<PlacedOrder>.Success(new PlacedOrder(existing.OrderNo, existing.Payment.Total));
    }

    private static bool IsDuplicateOrderNumber(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("uq_orders_order_no", StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase) == true;
}
