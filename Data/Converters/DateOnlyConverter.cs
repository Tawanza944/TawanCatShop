using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WebApplication1.Data.Converters;

/// <summary>
/// แมป <see cref="DateOnly"/> กับคอลัมน์ DATE ใน MySQL โดยตัวให้บริการ MySql.EntityFrameworkCore อ่าน DATE เป็น
/// <see cref="DateTime"/> จึงต้องแปลงชนิดก่อนใช้เพื่อเลี่ยง InvalidCastException
/// </summary>
public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(date => date.ToDateTime(TimeOnly.MinValue), value => DateOnly.FromDateTime(value))
    {
    }
}
