using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using ShopDotNet.Services;

namespace ShopDotNet.Controllers;

[Route("wishlist")]
public class WishlistController : BaseController
{
    public WishlistController(
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
        var user = CurrentUser;
        if (user == null) return Redirect("/login");

        var items = WishlistService.GetUserWishlist(user.Id);
        ViewData["Title"] = "My Wishlist";
        ViewData["Items"] = items;
        return View("~/Views/Account/Wishlist.cshtml");
    }

    [HttpPost("add")]
    public IActionResult Add([FromForm] int product_id, [FromForm] string csrf_token)
    {
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");
        var user = CurrentUser;
        
        if (user == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = false, redirect = "/login" });
            return Redirect("/login");
        }

        WishlistService.AddToWishlist(user.Id, product_id);
        
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { ok = true, message = "Added to wishlist." });

        Flash("wishlist_msg", "Added to wishlist.");
        return Redirect(Request.Headers["Referer"].ToString() ?? "/wishlist");
    }

    [HttpPost("remove")]
    public IActionResult Remove([FromForm] int product_id, [FromForm] string csrf_token)
    {
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");
        var user = CurrentUser;
        
        if (user == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = false, redirect = "/login" });
            return Redirect("/login");
        }

        WishlistService.RemoveFromWishlist(user.Id, product_id);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { ok = true, message = "Removed from wishlist." });

        Flash("wishlist_msg", "Removed from wishlist.");
        return Redirect("/wishlist");
    }
}
