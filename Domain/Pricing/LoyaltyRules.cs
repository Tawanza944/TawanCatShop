namespace WebApplication1.Domain.Pricing;

public sealed record LoyaltyBalance(decimal Available, decimal Progress)
{
    public decimal UntilNext => LoyaltyRules.SpendPerBaht - Progress;
}

public static class LoyaltyRules
{
    public const decimal SpendPerBaht = 1000m;

    public static LoyaltyBalance Balance(decimal eligibleSpend, decimal used) =>
        new(Math.Max(0, Math.Floor(eligibleSpend / SpendPerBaht) - used), eligibleSpend % SpendPerBaht);

    public static decimal Discount(decimal requested, decimal available, decimal subtotal, decimal couponDiscount) =>
        Math.Clamp(requested, 0, Math.Max(0, Math.Min(available, subtotal - couponDiscount)));

    public static decimal Allocate(decimal discount, decimal portion, decimal remainingSubtotal) =>
        remainingSubtotal == 0 ? 0 : Math.Round(discount * portion / remainingSubtotal, 2, MidpointRounding.AwayFromZero);
}
