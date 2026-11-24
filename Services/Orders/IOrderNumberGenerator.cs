namespace WebApplication1.Services.Orders;

/// <summary>Creates readable, per-day running order numbers such as CAT-20261123-0001.</summary>
public interface IOrderNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
