namespace WebApplication1.Domain.Orders;

public enum PaymentMethod
{
    PromptPay,
    CashOnDelivery
}

/// <summary>
/// Maps the payment methods the shop really accepts to the code the browser sends and the label stored in <c>orders.payment_method</c>.
/// </summary>
public static class PaymentMethods
{
    private static readonly IReadOnlyDictionary<PaymentMethod, (string Code, string Label)> Map =
        new Dictionary<PaymentMethod, (string, string)>
        {
            [PaymentMethod.PromptPay] = ("promptpay", "พร้อมเพย์"),
            [PaymentMethod.CashOnDelivery] = ("cod", "เก็บเงินปลายทาง")
        };

    public static IReadOnlyList<PaymentMethod> All { get; } = [PaymentMethod.PromptPay];

    public static string ToCode(this PaymentMethod method) => Map[method].Code;

    public static string ToThai(this PaymentMethod method) => Map[method].Label;

    public static bool TryParseCode(string? code, out PaymentMethod method)
    {
        foreach (var (key, value) in Map)
        {
            if (string.Equals(value.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                method = key;
                return true;
            }
        }

        method = default;
        return false;
    }
}
