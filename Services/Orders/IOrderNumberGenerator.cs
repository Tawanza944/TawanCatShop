namespace WebApplication1.Services.Orders;

/// <summary>สร้างเลขออเดอร์แบบมีลำดับรายวัน เช่น CAT-20261123-0001</summary>
public interface IOrderNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
