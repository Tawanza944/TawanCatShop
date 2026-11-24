using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Identity;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.Services.Catalog;
using WebApplication1.Services.Identity;
using WebApplication1.ViewModels;
using WebApplication1.ViewModels.Store;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Public storefront pages (home, products, product detail, promotions, help) and sign-in/sign-up.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthSession _authSession;
        private readonly IProductCatalogService _catalog;
        private readonly IReviewService _reviews;

        public HomeController(AppDbContext context, ICurrentUserService currentUser, IAuthSession authSession,
            IProductCatalogService catalog, IReviewService reviews)
        {
            _context = context;
            _currentUser = currentUser;
            _authSession = authSession;
            _catalog = catalog;
            _reviews = reviews;
        }

        private IActionResult LoginDestination(string? role) =>
            Roles.AreEqual(role, Roles.User)
                ? RedirectToAction(nameof(Index), "Home")
                : RedirectToAction(nameof(AccountController.Index), "Account");

        [HttpGet("/")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var all = await _catalog.GetCardsAsync(new ProductQuery(null, null, ProductSort.Recommended, null, null), cancellationToken);
            var model = new HomePageViewModel
            {
                Stats = await _catalog.GetStatsAsync(cancellationToken),
                Categories = await _catalog.GetCategoryCountsAsync(cancellationToken),
                BestSellers = all.OrderByDescending(p => p.Sold).ThenByDescending(p => p.Rating).Take(4).ToList(),
                CatPicks = all.Where(p => p.Category is { ForDogs: false }).Take(8).ToList(),
                DogPicks = all.Where(p => p.Category is { ForDogs: true }).Take(8).ToList(),
                Deals = all.Where(p => p.OnSale).Take(4).ToList(),
                FlashSales = all.Where(p => p.FlashEndsAt.HasValue).OrderBy(p => p.FlashEndsAt).ToList()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Product(string? category, string? search, ProductSort sort = ProductSort.Recommended,
            decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default)
            => View(await BuildCatalogAsync(null, category, search, sort, minPrice, maxPrice, cancellationToken));

        [HttpGet]
        public async Task<IActionResult> Shop(int id, string? category, string? search, ProductSort sort = ProductSort.Recommended,
            decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default)
        {
            var shop = await _catalog.GetShopAsync(id, cancellationToken);
            if (shop is null) return NotFound();
            return View(await BuildCatalogAsync(shop, category, search, sort, minPrice, maxPrice, cancellationToken));
        }

        private async Task<ProductListViewModel> BuildCatalogAsync(StorefrontShop? shop, string? category, string? search,
            ProductSort sort, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken)
        {
            var query = new ProductQuery(StorefrontCategories.FindByKey(category)?.Key, search?.Trim(), sort, minPrice, maxPrice, shop?.Id);
            var categories = shop is null
                ? await _catalog.GetCategoryCountsAsync(cancellationToken)
                : await _catalog.GetShopCategoryCountsAsync(shop.Id, cancellationToken);
            return new ProductListViewModel
            {
                Shop = shop,
                CanInteractWithShop = _currentUser.IsInRole(Roles.User),
                IsSignedIn = _currentUser.IsAuthenticated,
                IsFollowingShop = shop is not null && _currentUser.UserId is int userId &&
                    await _context.Set<ShopFollow>().AnyAsync(f => f.ShopId == shop.Id && f.UserId == userId, cancellationToken),
                Query = query,
                Products = await _catalog.GetCardsAsync(query, cancellationToken),
                Categories = categories,
                TotalProducts = categories.Sum(c => c.Count)
            };
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetail(int id, CancellationToken cancellationToken)
        {
            var card = await _catalog.GetCardAsync(id, cancellationToken);
            if (card is null)
            {
                return NotFound();
            }

            var reviews = await _reviews.GetForProductAsync(id, _currentUser.UserId, cancellationToken);
            var related = card.Category is null
                ? []
                : (await _catalog.GetCardsAsync(new ProductQuery(card.Category.Key, null, ProductSort.BestSelling, null, null), cancellationToken))
                    .Where(p => p.Id != id).Take(4).ToList();

            var model = new ProductDetailViewModel
            {
                Card = card,
                Description = await _catalog.GetDescriptionAsync(id, cancellationToken),
                Reviews = reviews,
                RatingBreakdown = Enumerable.Range(1, 5).Reverse().Select(star => reviews.Count(r => r.Rating == star)).ToList(),
                Eligibility = await _reviews.GetEligibilityAsync(id, _currentUser.UserId, cancellationToken),
                Related = related
            };
            return View(model);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Promotion(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var coupons = await _context.Coupons.AsNoTracking()
                .Where(c => c.IsActive && (!c.ExpiresAt.HasValue || c.ExpiresAt.Value >= now))
                .OrderBy(c => c.MinSpend)
                .ToListAsync(cancellationToken);

            var usedCodes = _currentUser.UserId is int userId
                ? (await _context.UserCoupons.AsNoTracking().Where(u => u.UserId == userId).Select(u => u.CouponCode).ToListAsync(cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var savedCouponIds = _currentUser.UserId is int customerId
                ? (await _context.Set<SavedCoupon>().Where(row => row.UserId == customerId)
                    .Select(row => row.CouponId).ToListAsync(cancellationToken)).ToHashSet()
                : new HashSet<int>();

            var deals = (await _catalog.GetCardsAsync(new ProductQuery(null, null, ProductSort.Recommended, null, null), cancellationToken))
                .Where(p => p.OnSale).Take(4).ToList();

            return View(new PromotionPageViewModel { Coupons = coupons, UsedCodes = usedCodes, SavedCouponIds = savedCouponIds, Deals = deals });
        }

        [HttpGet]
        public IActionResult Help() => View();

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_currentUser.IsAuthenticated)
            {
                return LoginDestination(_currentUser.Role);
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identifier = model.EmailOrUser.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "อีเมล/ชื่อผู้ใช้ หรือรหัสผ่านไม่ถูกต้อง");
                return View(model);
            }

            // ค้นหาบทบาท (Role) ของผู้ใช้จากตาราง staff
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (staff?.Status == StaffStatus.Suspended)
            {
                ModelState.AddModelError("", "บัญชีนี้ถูกระงับการใช้งาน กรุณาติดต่อผู้ดูแลร้าน");
                return View(model);
            }

            var roleName = Roles.User;
            if (staff != null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == staff.RoleId);
                if (role != null) roleName = role.Name;
            }

            _authSession.SignIn(user, roleName);
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                _authSession.SetAvatar(user.AvatarUrl);
            }

            TempData["SuccessMessage"] = $"ยินดีต้อนรับคุณ {user.Name} กลับมาแล้ว";
            return LoginDestination(roleName);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identifier = model.EmailOrUser.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

            if (user == null)
            {
                ModelState.AddModelError("EmailOrUser", "ไม่พบบัญชีผู้ใช้ที่ตรงกับอีเมลหรือชื่อผู้ใช้นี้ในระบบ LoveCat");
                return View(model);
            }

            // อัปเดตรหัสผ่านใหม่ที่ผ่านการเข้ารหัสแล้วลงในฐานข้อมูล MySQL จริง
            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"ตั้งค่ารหัสผ่านใหม่สำหรับ '{user.Username}' สำเร็จเรียบร้อยแล้ว! ✨ กรุณาเข้าสู่ระบบด้วยรหัสผ่านใหม่";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_currentUser.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameTrim = model.Username.Trim();
            var emailTrim = model.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == usernameTrim.ToLower()))
            {
                ModelState.AddModelError("Username", "ชื่อผู้ใช้ (Username) นี้มีผู้ใช้งานแล้ว");
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailTrim))
            {
                ModelState.AddModelError("Email", "อีเมลนี้ถูกลงทะเบียนไว้แล้ว");
                return View(model);
            }

            // บันทึกข้อมูลผู้ใช้ใหม่ลงในตาราง users ของ MySQL
            var user = new User
            {
                Username = usernameTrim,
                Name = model.Name.Trim(),
                Email = emailTrim,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // เชื่อมโยงบทบาทเริ่มต้นของผู้ใช้ในตาราง staff (RoleId = 2 คือสมาชิกทั่วไป User)
            _context.Staff.Add(new Staff
            {
                UserId = user.Id,
                RoleId = 2,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบด้วยบัญชีใหม่ของคุณ";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authSession.SignOut();
            TempData["SuccessMessage"] = "ออกจากระบบเรียบร้อยแล้ว";
            return RedirectToAction("Login");
        }
    }
}
