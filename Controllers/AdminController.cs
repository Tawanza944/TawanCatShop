using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Backoffice;
using WebApplication1.ViewModels.Backoffice;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// รับคำสั่งจากหลังบ้านแอดมินด้วย Post/Redirect/Get และมีหน้า Adduser กับ userlist แยกสำหรับจัดการผู้ใช้
    /// ทั้งสองหน้าเรียก <see cref="IUserAdminService"/> เพื่อใช้กฎจัดการบัญชีชุดเดียวกัน
    /// </summary>
    [RequireRole(Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserAdminService _users;
        private readonly ICouponAdminService _coupons;

        public AdminController(AppDbContext context, IUserAdminService users, ICouponAdminService coupons)
        {
            _context = context;
            _users = users;
            _coupons = coupons;
        }

        // ---------- คำสั่งจากหน้า Dashboard ----------

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFlashSale([FromServices] FlashSaleAdmin flash, [FromForm] FlashSaleDraft draft, CancellationToken token)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            string? error;
            if (!int.TryParse(draft.ProductId, out var productId)) error = "เลือกสินค้าก่อนสร้าง Flash Sale";
            else if (!decimal.TryParse(draft.Price, System.Globalization.NumberStyles.AllowDecimalPoint, culture, out var price)) error = "กรอกราคาพิเศษเป็นตัวเลข";
            else if (!TryBangkokTime(draft.StartsAt, out var startsAt)) error = "เลือกวันที่และเวลาเริ่ม";
            else if (!TryBangkokTime(draft.EndsAt, out var endsAt)) error = "เลือกวันที่และเวลาสิ้นสุด";
            else if (!int.TryParse(draft.Quota, out var quota)) error = "กรอกโควตาเป็นจำนวนเต็ม";
            else error = (await flash.CreateAsync(productId, price, startsAt, endsAt, quota, token)).Error;

            if (error is not null)
            {
                TempData["ErrorMessage"] = error;
                TempData["FlashSaleDraft"] = System.Text.Json.JsonSerializer.Serialize(draft);
            }
            else TempData["SuccessMessage"] = "สร้าง Flash Sale แล้ว";
            return RedirectToAction(nameof(AccountController.FlashSales), "Account");
        }

        private static bool TryBangkokTime(string value, out DateTime utc)
        {
            utc = default;
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd'T'HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var local) || local.Year < 2000) return false;
            utc = new DateTimeOffset(local, TimeSpan.FromHours(7)).UtcDateTime;
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUser(UserForm form, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return Back("users", FirstError());
            var result = await _users.SaveAsync(form, cancellationToken);
            return Back("users", result.Error, $"บันทึกผู้ใช้ {form.Username} แล้ว");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSuspended(int id, bool suspended, CancellationToken cancellationToken)
        {
            var result = await _users.SetSuspendedAsync(id, suspended, cancellationToken);
            return Back("users", result.Error, suspended ? "ระงับบัญชีแล้ว" : "เปิดใช้บัญชีแล้ว");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var result = await _users.DeleteAsync(id, cancellationToken);
            return Back("users", result.Error, "ลบผู้ใช้แล้ว");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCoupon(CouponForm form, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return Back("coupons", FirstError());
            var result = await _coupons.SaveAsync(form, cancellationToken);
            return Back("coupons", result.Error, $"บันทึกโค้ด {form.Code.ToUpperInvariant()} แล้ว");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCoupon(int id, CancellationToken cancellationToken)
        {
            var result = await _coupons.ToggleAsync(id, cancellationToken);
            return Back("coupons", result.Error, "เปลี่ยนสถานะโค้ดแล้ว");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoupon(int id, CancellationToken cancellationToken)
        {
            var result = await _coupons.DeleteAsync(id, cancellationToken);
            return Back("coupons", result.Error, "ลบโค้ดแล้ว");
        }

        // ---------- หน้าแยกสำหรับจัดการผู้ใช้ ----------

        [HttpGet]
        public async Task<IActionResult> Adduser(CancellationToken cancellationToken)
        {
            ViewBag.AvailableRoles = await RoleNamesAsync(cancellationToken);
            return View(new UserForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adduser(UserForm form, CancellationToken cancellationToken)
        {
            ViewBag.AvailableRoles = await RoleNamesAsync(cancellationToken);
            if (!ModelState.IsValid) return View(form);

            form.Id = 0;
            var result = await _users.SaveAsync(form, cancellationToken);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(form);
            }

            TempData["SuccessMessage"] = $"เพิ่มผู้ใช้ {form.Username} ({form.Role}) แล้ว";
            return RedirectToAction(nameof(Adduser));
        }

        [HttpGet]
        public async Task<IActionResult> userlist(CancellationToken cancellationToken)
        {
            var rows = await (from user in _context.Users.AsNoTracking()
                              join staff in _context.Staff on user.Id equals staff.UserId into memberships
                              from membership in memberships.DefaultIfEmpty()
                              join role in _context.Roles on membership.RoleId equals role.Id into roles
                              from assigned in roles.DefaultIfEmpty()
                              orderby user.Id descending
                              select new { User = user, Role = assigned == null ? Roles.User : assigned.Name })
                             .ToListAsync(cancellationToken);

            foreach (var row in rows) row.User.Role = row.Role;

            ViewBag.AvailableRoles = await RoleNamesAsync(cancellationToken);
            return View(rows.Select(r => r.User).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, string role, CancellationToken cancellationToken)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user is null)
            {
                TempData["ErrorMessage"] = "ไม่พบผู้ใช้นี้";
                return RedirectToAction(nameof(userlist));
            }

            var form = new UserForm { Id = user.Id, Username = user.Username, Name = user.Name, Email = user.Email, PhoneNumber = user.PhoneNumber, Role = role };
            var result = await _users.SaveAsync(form, cancellationToken);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
                result.Succeeded ? $"เปลี่ยนบทบาทของ {user.Username} เป็น {role} แล้ว" : result.Error;
            return RedirectToAction(nameof(userlist));
        }

        private Task<List<string>> RoleNamesAsync(CancellationToken cancellationToken) =>
            _context.Roles.OrderBy(r => r.Id).Select(r => r.Name).ToListAsync(cancellationToken);

        private string FirstError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "ข้อมูลไม่ถูกต้อง";

        /// <summary>ตั้งข้อความสำเร็จหรือข้อผิดพลาด แล้วกลับไปส่วนหลังบ้านที่เกี่ยวข้อง</summary>
        private IActionResult Back(string section, string? error, string? success = null)
        {
            TempData[error is null ? "SuccessMessage" : "ErrorMessage"] = error ?? success;
            return RedirectToAction(WebApplication1.ViewModels.Dashboard.DashboardRoutes.ActionFor(section), "Account");
        }
    }
}
