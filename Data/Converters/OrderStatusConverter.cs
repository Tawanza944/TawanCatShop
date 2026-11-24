using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Data.Converters;

/// <summary>
/// บันทึก <see cref="OrderStatus"/> ด้วยรหัสตัวพิมพ์เล็กเดิม เช่น "pending" และ "delivered"
/// จึงยังอ่านข้อมูลที่สร้างก่อนประกาศ enum นี้ได้
/// </summary>
public sealed class OrderStatusConverter : ValueConverter<OrderStatus, string>
{
    public OrderStatusConverter()
        : base(status => status.ToCode(), code => OrderStatusCodes.ParseCode(code))
    {
    }
}
