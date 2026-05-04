using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using ShopDotNet.Services;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/reviews")]
public class AdminReviewsController : AdminBaseController
{
    public AdminReviewsController(
        IDatabaseService db,
        ISettingsService settings,
        ICartService cart,
        IAuthService auth,
        ISecurityService security,
        IReviewService reviews,
        IWishlistService wishlist,
        IAddressService addresses,
        IAttributeService attributes)
        : base(db, settings, cart, auth, security, reviews, wishlist, addresses, attributes)
    {
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var allReviews = ReviewService.GetAllReviews();
        ViewData["Title"] = "Manage Reviews";
        ViewData["Active"] = "reviews";
        ViewData["Reviews"] = allReviews;
        ViewData["FlashMsg"] = GetFlash("review_saved");
        return View("~/Views/Admin/Reviews/Index.cshtml");
    }

    [HttpPost("status")]
    public IActionResult UpdateStatus([FromForm] int id, [FromForm] string status, [FromForm] string csrf_token)
    {
        if (!ValidateCsrf(csrf_token)) return BadRequest();
        
        ReviewService.UpdateReviewStatus(id, status);
        Flash("review_saved", $"Review marked as {status}.");
        return Redirect("/admin/reviews");
    }
}
