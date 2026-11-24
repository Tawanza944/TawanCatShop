namespace WebApplication1.Services.Backoffice;

public enum LedgerLineKind
{
    CustomerPayment,
    ShopPayout,
    RiderFee,
    ManualIncome,
    ManualExpense,
    ShippingCollected,
    CouponCost,
    FlashSaleCost,
    RewardsCost
}

/// <summary>หนึ่งบรรทัดในสมุดบัญชี มาจากการคำนวณออเดอร์หรือรายการใน ledger_entries</summary>
public sealed record LedgerLine(
    DateTime At,
    string Reference,
    string Title,
    string? Party,
    decimal MoneyIn,
    decimal MoneyOut,
    LedgerLineKind Kind,
    int? EntryId = null,
    bool AwaitingCommission = false)
{
    public decimal Balance { get; init; }
}

public sealed record LedgerSummary(decimal MoneyIn, decimal MoneyOut, decimal ShopPayouts, decimal RiderFees, decimal CommissionEarned, int AwaitingCommission,
    decimal CouponCost, decimal FlashSaleCost, decimal RewardsCost, decimal ManualExpenses, decimal ShippingCollected, decimal OutstandingPayout)
{
    public decimal Balance => MoneyIn - MoneyOut;
    public decimal NetShippingCost => Math.Max(0, RiderFees - ShippingCollected);
    public decimal PlatformExpenses => CouponCost + FlashSaleCost + RewardsCost + ManualExpenses + NetShippingCost;
    public decimal PlatformNet => CommissionEarned - PlatformExpenses;
}

public sealed record LedgerReport(DateOnly From, DateOnly To, IReadOnlyList<LedgerLine> Lines, LedgerSummary Summary);

public sealed record CommissionRow(int OrderId, string OrderNo, string CustomerName, DateTime DeliveredAt, decimal Total, decimal Commission, decimal ShopNet);
