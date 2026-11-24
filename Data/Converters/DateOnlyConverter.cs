using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WebApplication1.Data.Converters;

/// <summary>
/// Maps <see cref="DateOnly"/> to a MySQL DATE column. The MySql.EntityFrameworkCore provider reads DATE as
/// <see cref="DateTime"/>, so without this conversion reading the column throws InvalidCastException.
/// </summary>
public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(date => date.ToDateTime(TimeOnly.MinValue), value => DateOnly.FromDateTime(value))
    {
    }
}
