using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Addresses;
using WebApplication1.Models;

namespace WebApplication1.Services.Accounts;

public sealed partial class AddressService : IAddressService
{
    private const int MaxAddressesPerUser = 5;

    private readonly AppDbContext _db;

    public AddressService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserAddress>> GetForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _db.UserAddresses.AsNoTracking().Include(a => a.Region)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public async Task<OperationResult<int>> SaveAsync(int userId, UserAddress input, CancellationToken cancellationToken = default)
    {
        var error = Validate(input);
        if (error is not null)
        {
            return OperationResult<int>.Failure(error);
        }

        UserAddress address;
        if (input.Id == 0)
        {
            var count = await _db.UserAddresses.CountAsync(a => a.UserId == userId, cancellationToken);
            if (count >= MaxAddressesPerUser)
            {
                return OperationResult<int>.Failure($"บันทึกที่อยู่ได้สูงสุด {MaxAddressesPerUser} ที่");
            }

            address = new UserAddress { UserId = userId, IsDefault = count == 0 };
            _db.UserAddresses.Add(address);
        }
        else
        {
            var existing = await _db.UserAddresses.Include(a => a.Region).FirstOrDefaultAsync(a => a.Id == input.Id && a.UserId == userId, cancellationToken);
            if (existing is null)
            {
                return OperationResult<int>.Failure("ไม่พบที่อยู่นี้");
            }

            address = existing;
        }

        address.Label = input.Label.Trim();
        address.RecipientName = input.RecipientName.Trim();
        address.Phone = input.Phone.Trim();
        address.AddressLine = input.AddressLine.Trim();
        address.Region ??= new SavedAddressRegion();
        address.Region.CopyFrom(input.Region!);

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Success(address.Id);
    }

    public async Task<OperationResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken = default)
    {
        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);
        if (address is null)
        {
            return OperationResult.Fail("ไม่พบที่อยู่นี้");
        }

        _db.UserAddresses.Remove(address);
        if (address.IsDefault)
        {
            var next = await _db.UserAddresses.Where(a => a.UserId == userId && a.Id != addressId).OrderBy(a => a.Id).FirstOrDefaultAsync(cancellationToken);
            if (next is not null)
            {
                next.IsDefault = true;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> SetDefaultAsync(int userId, int addressId, CancellationToken cancellationToken = default)
    {
        var addresses = await _db.UserAddresses.Where(a => a.UserId == userId).ToListAsync(cancellationToken);
        if (addresses.All(a => a.Id != addressId))
        {
            return OperationResult.Fail("ไม่พบที่อยู่นี้");
        }

        foreach (var address in addresses)
        {
            address.IsDefault = address.Id == addressId;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    private static string? Validate(UserAddress input)
    {
        if (string.IsNullOrWhiteSpace(input.Label) || input.Label.Trim().Length > 50) return "ตั้งชื่อที่อยู่ เช่น บ้าน หรือ ที่ทำงาน (ไม่เกิน 50 ตัว)";
        if (string.IsNullOrWhiteSpace(input.RecipientName) || input.RecipientName.Trim().Length < 2) return "กรอกชื่อผู้รับ";
        if (!ThaiMobile().IsMatch(input.Phone?.Trim() ?? string.Empty)) return "เบอร์มือถือต้องมี 10 หลักและขึ้นต้นด้วย 0";
        if (string.IsNullOrWhiteSpace(input.AddressLine) || input.AddressLine.Trim().Length < 15) return "ที่อยู่ต้องยาวอย่างน้อย 15 ตัวอักษร";
        return ThailandAddressCatalog.Format(input.Region?.Value) is null ? "เลือกจังหวัด อำเภอ ตำบล และรหัสไปรษณีย์ให้ตรงกัน" : null;
    }

    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex ThaiMobile();
}
