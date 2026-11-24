using Microsoft.AspNetCore.Mvc;
using WebApplication1.Filters;
using WebApplication1.Services.Catalog;
using WebApplication1.Services.Identity;

namespace WebApplication1.Controllers;

public sealed class ReviewsController : Controller
{
    private readonly IReviewService _reviews;
    private readonly ICurrentUserService _currentUser;

    public ReviewsController(IReviewService reviews, ICurrentUserService currentUser)
    {
        _reviews = reviews;
        _currentUser = currentUser;
    }

    [HttpPost]
    [RequireLogin]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int productId, int rating, string? comment, CancellationToken cancellationToken)
    {
        var result = await _reviews.SaveAsync(_currentUser.RequireUserId(), productId, rating, comment, cancellationToken);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "บันทึกรีวิวแล้ว ขอบคุณที่บอกต่อ" : result.Error;
        return RedirectToAction("ProductDetail", "Home", new { id = productId }, fragment: "reviews");
    }
}
