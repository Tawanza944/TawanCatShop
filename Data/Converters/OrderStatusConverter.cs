using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Data.Converters;

/// <summary>
/// Stores <see cref="OrderStatus"/> as its existing lowercase code ("pending", "delivered", ...)
/// so rows written before the enum existed keep working.
/// </summary>
public sealed class OrderStatusConverter : ValueConverter<OrderStatus, string>
{
    public OrderStatusConverter()
        : base(status => status.ToCode(), code => OrderStatusCodes.ParseCode(code))
    {
    }
}
