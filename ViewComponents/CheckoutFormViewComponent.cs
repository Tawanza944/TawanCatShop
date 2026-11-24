using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Services.Accounts;
using WebApplication1.Services.Identity;

namespace WebApplication1.ViewComponents;

public sealed record CheckoutFormViewModel(
    bool IsAuthenticated,
    string? DefaultName,
    string? DefaultPhone,
    IReadOnlyList<UserAddress> Addresses,
    IReadOnlyList<PaymentMethod> PaymentMethods);

/// <summary>เตรียมฟอร์มชำระเงินและที่อยู่ที่ลูกค้าเคยบันทึก</summary>
public sealed class CheckoutFormViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAddressService _addresses;
    private readonly AppDbContext _db;

    public CheckoutFormViewComponent(ICurrentUserService currentUser, IAddressService addresses, AppDbContext db)
    {
        _currentUser = currentUser;
        _addresses = addresses;
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (_currentUser.UserId is not int userId)
        {
            return View(new CheckoutFormViewModel(false, null, null, [], PaymentMethods.All));
        }

        var user = await _db.Users.AsNoTracking().Where(u => u.Id == userId)
            .Select(u => new { u.Name, u.PhoneNumber }).FirstOrDefaultAsync();
        var addresses = await _addresses.GetForUserAsync(userId);

        return View(new CheckoutFormViewModel(true, user?.Name, user?.PhoneNumber, addresses, PaymentMethods.All));
    }
}
