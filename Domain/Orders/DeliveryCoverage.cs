namespace WebApplication1.Domain.Orders;

public static class DeliveryCoverage
{
    public const string BangkokCode = "10";
    public const string BangkokName = "กรุงเทพฯ";

    public static bool UsesRider(string? provinceCode) => provinceCode == BangkokCode;

    public static string? ProvinceName(string? provinceCode) => provinceCode switch
    {
        BangkokCode => BangkokName,
        "12" => "นนทบุรี",
        "13" => "ปทุมธานี",
        _ => null
    };
}
