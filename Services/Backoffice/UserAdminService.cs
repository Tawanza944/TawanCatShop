using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Common;
using WebApplication1.Domain.Identity;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Services.Backoffice;

public sealed class UserAdminService : IUserAdminService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthSession _authSession;

    public UserAdminService(AppDbContext db, ICurrentUserService currentUser, IAuthSession authSession)
    {
        _db = db;
        _currentUser = currentUser;
        _authSession = authSession;
    }

    private string Actor => _currentUser.UserName ?? string.Empty;

    public async Task<OperationResult<int>> SaveAsync(UserForm form, CancellationToken cancellationToken = default)
    {
        // Owner membership can only be established outside the Admin user-management flow.
        var targetIsOwner = form.Id > 0 && await _db.Staff.AnyAsync(staff => staff.UserId == form.Id &&
            _db.Roles.Any(role => role.Id == staff.RoleId && role.Name == Roles.Owner), cancellationToken);
        if (targetIsOwner || Roles.AreEqual(form.Role, Roles.Owner))
        {
            return OperationResult<int>.Failure("แก้ไขบทบาท Owner ผ่านการจัดการผู้ใช้ของ Admin ไม่ได้");
        }

        var username = form.Username.Trim();
        var email = form.Email.Trim().ToLowerInvariant();

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == form.Role, cancellationToken);
        if (role is null)
        {
            return OperationResult<int>.Failure("บทบาทนี้ไม่มีในระบบ");
        }

        if (await _db.Users.AnyAsync(u => u.Id != form.Id && u.Username == username, cancellationToken))
        {
            return OperationResult<int>.Failure($"ชื่อผู้ใช้ {username} มีคนใช้แล้ว");
        }

        if (await _db.Users.AnyAsync(u => u.Id != form.Id && u.Email == email, cancellationToken))
        {
            return OperationResult<int>.Failure($"อีเมล {email} มีคนใช้แล้ว");
        }

        User user;
        if (form.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(form.Password))
            {
                return OperationResult<int>.Failure("ตั้งรหัสผ่านเริ่มต้นให้ผู้ใช้ใหม่");
            }

            user = new User { PasswordHash = PasswordHelper.HashPassword(form.Password), CreatedAt = DateTime.UtcNow };
            _db.Users.Add(user);
        }
        else
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == form.Id, cancellationToken);
            if (existing is null)
            {
                return OperationResult<int>.Failure("ไม่พบผู้ใช้นี้");
            }

            user = existing;
        }

        user.Username = username;
        user.Name = form.Name.Trim();
        user.Email = email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(form.PhoneNumber) ? null : form.PhoneNumber.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
        if (staff is null)
        {
            _db.Staff.Add(new Staff { UserId = user.Id, RoleId = role.Id, Status = StaffStatus.Active, CreatedAt = DateTime.UtcNow });
        }
        else
        {
            staff.RoleId = role.Id;
        }

        _db.RecordAdminActivity(Actor, $"{(form.Id == 0 ? "เพิ่ม" : "แก้ไข")}ผู้ใช้ {username} ({role.Name})");
        await _db.SaveChangesAsync(cancellationToken);

        if (_currentUser.UserId == user.Id)
        {
            _authSession.RefreshProfile(user);
            _authSession.SetRole(role.Name);
        }

        return OperationResult<int>.Success(user.Id);
    }

    public async Task<OperationResult<bool>> DemoteAdminAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsInRole(Roles.Owner))
            return OperationResult.Fail("เฉพาะ Owner ที่ลดสิทธิ์ Admin ได้");

        var username = await _db.Users.Where(u => u.Id == userId).Select(u => u.Username).SingleOrDefaultAsync(cancellationToken);
        if (username is null)
            return OperationResult.Fail("ไม่พบผู้ใช้นี้");

        var roleIds = await _db.Roles.Where(role => role.Name == Roles.Admin || role.Name == Roles.User)
            .ToDictionaryAsync(role => role.Name, role => role.Id, cancellationToken);
        if (!roleIds.TryGetValue(Roles.Admin, out var adminId) || !roleIds.TryGetValue(Roles.User, out var customerId))
            return OperationResult.Fail("ข้อมูลบทบาท Admin หรือ User ไม่ครบ");

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var changed = await _db.Staff.Where(staff => staff.UserId == userId && staff.RoleId == adminId)
            .ExecuteUpdateAsync(update => update.SetProperty(staff => staff.RoleId, customerId), cancellationToken);
        if (changed != 1)
            return OperationResult.Fail("บัญชีนี้ไม่ได้เป็น Admin แล้ว");

        _db.RecordAdminActivity(Actor, $"ลดสิทธิ์ {username} จาก Admin เป็น User");
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> SetSuspendedAsync(int userId, bool suspended, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == userId)
        {
            return OperationResult.Fail("ระงับบัญชีที่ตัวเองใช้อยู่ไม่ได้");
        }

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var username = await _db.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefaultAsync(cancellationToken);
        if (username is null)
        {
            return OperationResult.Fail("ไม่พบผู้ใช้นี้");
        }

        if (staff is null)
        {
            var userRole = await _db.Roles.FirstAsync(r => r.Name == Roles.User, cancellationToken);
            staff = new Staff { UserId = userId, RoleId = userRole.Id, CreatedAt = DateTime.UtcNow };
            _db.Staff.Add(staff);
        }

        staff.Status = suspended ? StaffStatus.Suspended : StaffStatus.Active;
        _db.RecordAdminActivity(Actor, $"{(suspended ? "ระงับ" : "เปิดใช้")}บัญชี {username}");
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<bool>> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == userId)
        {
            return OperationResult.Fail("ลบบัญชีที่ตัวเองใช้อยู่ไม่ได้");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("ไม่พบผู้ใช้นี้");
        }

        if (await _db.Orders.AnyAsync(o => o.UserId == userId || o.RiderId == userId, cancellationToken))
        {
            return OperationResult.Fail("ผู้ใช้นี้มีประวัติคำสั่งซื้อ ให้ใช้การระงับบัญชีแทนการลบ");
        }

        _db.Users.Remove(user);
        _db.RecordAdminActivity(Actor, $"ลบผู้ใช้ {user.Username}");
        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok();
    }
}
