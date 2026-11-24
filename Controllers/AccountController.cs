using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Identity;
using WebApplication1.Filters;
using WebApplication1.Services.Accounts;
using WebApplication1.Services.Dashboards;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels.Account;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// แสดงหน้าโปรไฟล์และเลือก Dashboard ให้ตรงกับบทบาทของผู้ใช้ที่ล็อกอิน
    /// คำสั่งเฉพาะบทบาทอยู่ใน AdminController, ShopController, AccountingController และ OrdersController
    /// </summary>
    [RequireLogin]
    public class AccountController : Controller
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IEnumerable<IRoleDashboard> _dashboards;
        private readonly IProfileService _profiles;
        private readonly IAddressService _addresses;
        private readonly AppDbContext _db;

        public AccountController(ICurrentUserService currentUser, IEnumerable<IRoleDashboard> dashboards,
            IProfileService profiles, IAddressService addresses, AppDbContext db)
        {
            _currentUser = currentUser;
            _dashboards = dashboards;
            _profiles = profiles;
            _addresses = addresses;
            _db = db;
        }

        [HttpGet("Account/Index")]
        public Task<IActionResult> Index(CancellationToken cancellationToken) =>
            DashboardAsync("overview", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/Product")]
        public Task<IActionResult> Product([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("products", query, cancellationToken);

        [HttpGet("Account/Packing")]
        public Task<IActionResult> Packing([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("packing", query, cancellationToken);

        [HttpGet("Account/Shipped")]
        public Task<IActionResult> Shipped([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("shipped", query, cancellationToken);

        [HttpGet("Account/Revenue")]
        public Task<IActionResult> Revenue([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("revenue", query, cancellationToken);

        [HttpGet("Account/Tracking")]
        public Task<IActionResult> Tracking([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("tracking", query, cancellationToken);

        [HttpGet("Account/Orders")]
        public Task<IActionResult> Orders([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("orders", query, cancellationToken);

        [HttpGet("Account/Coupons")]
        public Task<IActionResult> Coupons(CancellationToken cancellationToken) =>
            DashboardAsync("coupons", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/FlashSales")]
        [RequireRole(Roles.Admin)]
        public Task<IActionResult> FlashSales(CancellationToken cancellationToken) =>
            DashboardAsync("flash-sales", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/Rewards")]
        [RequireRole(Roles.User)]
        public Task<IActionResult> Rewards(CancellationToken cancellationToken) =>
            DashboardAsync("rewards", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/Jobs")]
        public Task<IActionResult> Jobs(CancellationToken cancellationToken) =>
            DashboardAsync("jobs", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/MyDeliveries")]
        public Task<IActionResult> MyDeliveries(CancellationToken cancellationToken) =>
            DashboardAsync("mine", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/History")]
        public Task<IActionResult> History([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("history", query, cancellationToken);

        [HttpGet("Account/OrderStatus")]
        [RequireRole(Roles.Accounting)]
        public Task<IActionResult> OrderStatus([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("order-status", query, cancellationToken);

        [HttpGet("Account/Commission")]
        public Task<IActionResult> Commission(CancellationToken cancellationToken) =>
            DashboardAsync("commission", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/Ledger")]
        public Task<IActionResult> Ledger([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("ledger", query, cancellationToken);

        [HttpGet("Account/Sales")]
        public Task<IActionResult> Sales([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("sales", query, cancellationToken);

        [HttpGet("Account/Finance")]
        public Task<IActionResult> Finance([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("finance", query, cancellationToken);

        [HttpGet("Account/Ranking")]
        public Task<IActionResult> Ranking(CancellationToken cancellationToken) =>
            DashboardAsync("ranking", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/AdminAccess")]
        [RequireRole(Roles.Owner)]
        public Task<IActionResult> AdminAccess([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("admin-access", query, cancellationToken);

        [HttpGet("Account/Shops")]
        [RequireRole(Roles.Admin)]
        public Task<IActionResult> Shops(CancellationToken cancellationToken) =>
            DashboardAsync("shops", new DashboardQuery(), cancellationToken);

        [HttpGet("Account/Users")]
        public Task<IActionResult> Users([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("users", query, cancellationToken);

        [HttpGet("Account/Activity")]
        public Task<IActionResult> Activity([FromQuery] DashboardQuery query, CancellationToken cancellationToken) =>
            DashboardAsync("activity", query, cancellationToken);

        private async Task<IActionResult> DashboardAsync(string section, DashboardQuery query, CancellationToken cancellationToken)
        {
            var dashboard = DashboardForCurrentUser();

            var request = new DashboardRequest(_currentUser.RequireUserId(), section, query.PageNumber, query.Search, query.Filter, query.From, query.To, query.Payout);
            var page = await dashboard.BuildAsync(request, cancellationToken);
            return page is null ? NotFound() : View(page);
        }

        [HttpGet("Account/Profile")]
        public async Task<IActionResult> Profile(string tab = "details", CancellationToken cancellationToken = default)
        {
            if (tab is "addresses" or "orders" && !_currentUser.IsInRole(Roles.User))
                return StatusCode(StatusCodes.Status403Forbidden);
            var model = await BuildProfileAsync(tab, cancellationToken);
            return model is null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileForm form, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProfileError("details");
            }

            var result = await _profiles.UpdateDetailsAsync(_currentUser.RequireUserId(), form, cancellationToken);
            return ProfileResult("details", result.Succeeded, "บันทึกข้อมูลแล้ว", result.Error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateAvatar(IFormFile? avatar, CancellationToken cancellationToken)
        {
            if (avatar is null)
            {
                return ProfileResult("details", false, null, "เลือกรูปก่อนกดอัปโหลด");
            }

            var result = await _profiles.UpdateAvatarAsync(_currentUser.RequireUserId(), avatar, cancellationToken);
            return ProfileResult("details", result.Succeeded, "เปลี่ยนรูปโปรไฟล์แล้ว", result.Error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordChangeForm form, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProfileError("password");
            }

            var result = await _profiles.ChangePasswordAsync(_currentUser.RequireUserId(), form, cancellationToken);
            return ProfileResult("password", result.Succeeded, "เปลี่ยนรหัสผ่านแล้ว", result.Error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole(Roles.User)]
        public async Task<IActionResult> SaveAddress(AddressForm form, CancellationToken cancellationToken, bool checkout = false)
        {
            var userId = _currentUser.RequireUserId();
            if (!ModelState.IsValid) return checkout ? BadRequest(new { message = "กรอกข้อมูลที่อยู่ให้ครบ" }) : ProfileResult("addresses", false, null, "กรอกข้อมูลที่อยู่ให้ครบ");
            var result = await _addresses.SaveAsync(userId, form.ToEntity(), cancellationToken);
            if (checkout) return await CheckoutAddresses(userId, result.Error, result.Value, cancellationToken);
            return ProfileResult("addresses", result.Succeeded, "บันทึกที่อยู่แล้ว", result.Error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole(Roles.User)]
        public async Task<IActionResult> DeleteAddress(int id, CancellationToken cancellationToken, bool checkout = false)
        {
            var userId = _currentUser.RequireUserId();
            var result = await _addresses.DeleteAsync(userId, id, cancellationToken);
            if (checkout) return await CheckoutAddresses(userId, result.Error, null, cancellationToken);
            return ProfileResult("addresses", result.Succeeded, "ลบที่อยู่แล้ว", result.Error);
        }

        private async Task<IActionResult> CheckoutAddresses(int userId, string? error, int? selectedId, CancellationToken cancellationToken)
        {
            if (error is not null) return BadRequest(new { message = error });
            if (selectedId is int id) Response.Headers["X-Saved-Address-Id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return PartialView("~/Views/Shared/_CheckoutAddresses.cshtml", await _addresses.GetForUserAsync(userId, cancellationToken));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole(Roles.User)]
        public async Task<IActionResult> SetDefaultAddress(int id, CancellationToken cancellationToken)
        {
            var result = await _addresses.SetDefaultAsync(_currentUser.RequireUserId(), id, cancellationToken);
            return ProfileResult("addresses", result.Succeeded, "ตั้งเป็นที่อยู่หลักแล้ว", result.Error);
        }

        private IActionResult ProfileResult(string tab, bool succeeded, string? success, string? error)
        {
            TempData[succeeded ? "SuccessMessage" : "ErrorMessage"] = succeeded ? success : error;
            return RedirectToAction(nameof(Profile), new { tab });
        }

        private IActionResult ProfileError(string tab)
        {
            var message = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrEmpty(m));
            return ProfileResult(tab, false, null, message ?? "ข้อมูลไม่ถูกต้อง");
        }

        private IRoleDashboard DashboardForCurrentUser() =>
            _dashboards.FirstOrDefault(d => Roles.AreEqual(d.Role, _currentUser.Role))
            ?? _dashboards.First(d => d.Role == Roles.User);

        private async Task<ProfilePageViewModel?> BuildProfileAsync(string tab, CancellationToken cancellationToken)
        {
            var userId = _currentUser.RequireUserId();
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is null)
            {
                return null;
            }

            var dashboard = DashboardForCurrentUser();
            var customer = _currentUser.IsInRole(Roles.User);
            var orders = _db.Orders.AsNoTracking().Where(o => o.UserId == userId);
            return new ProfilePageViewModel
            {
                Tab = tab is "details" or "password" or "addresses" or "orders" ? tab : "details",
                User = user,
                Sidebar = new DashboardSidebar(RoleProfiles.For(dashboard.Role), dashboard.Menu, DashboardSidebar.ProfileSection, user.Name),
                Addresses = customer ? await _addresses.GetForUserAsync(userId, cancellationToken) : [],
                RecentOrders = customer ? await orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).Take(3).ToListAsync(cancellationToken) : [],
                OrderCount = customer ? await orders.CountAsync(o => o.Status != Domain.Orders.OrderStatus.Cancelled, cancellationToken) : 0,
                Profile = new ProfileForm { Name = user.Name, Username = user.Username, Email = user.Email, PhoneNumber = user.PhoneNumber }
            };
        }
    }
}
