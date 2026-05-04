using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using ShopDotNet.Services;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class AccountController : BaseController
{
    public AccountController(
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

    [HttpGet("account")]
    public IActionResult Index()
    {
        var user = CurrentUser;
        if (user == null)
        {
            HttpContext.Session.SetString("redirect_after_login", "/account");
            return Redirect("/login");
        }

        using var conn = Db.GetConnection();
        var orders = conn.Query<Order>(@"
            SELECT o.*, COUNT(oi.id) as ItemCount
            FROM orders o
            LEFT JOIN order_items oi ON oi.order_id = o.id
            WHERE o.user_id=@userId
            GROUP BY o.id
            ORDER BY o.created_at DESC
            LIMIT 5",
            new { userId = user.Id }).ToList();

        var addresses = AddressService.GetUserAddresses(user.Id);

        ViewData["Title"] = "My Account";
        ViewData["Orders"] = orders;
        ViewData["Addresses"] = addresses;
        return View();
    }

    [HttpGet("account/addresses")]
    public IActionResult Addresses()
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");

        var userAddresses = AddressService.GetUserAddresses(user.Id);
        ViewData["Title"] = "My Addresses";
        ViewData["Addresses"] = userAddresses;
        return View();
    }

    [HttpPost("account/addresses/save")]
    public IActionResult SaveAddress([FromForm] UserAddress model, [FromForm] string? csrf_token)
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");

        model.UserId = user.Id;
        if (model.Id > 0)
        {
            AddressService.UpdateAddress(model);
        }
        else
        {
            AddressService.AddAddress(model);
        }

        Flash("address_saved", "1");
        return Redirect("/account/addresses");
    }

    [HttpPost("account/addresses/default")]
    public IActionResult SetDefaultAddress([FromForm] int id, [FromForm] string? csrf_token)
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");

        AddressService.SetDefaultAddress(id, user.Id);
        Flash("address_saved", "1");
        return Redirect("/account/addresses");
    }

    [HttpPost("account/addresses/delete")]
    public IActionResult DeleteAddress([FromForm] int id, [FromForm] string? csrf_token)
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");

        AddressService.DeleteAddress(id, user.Id);
        Flash("address_saved", "1");
        return Redirect("/account/addresses");
    }

    [HttpGet("account/wishlist")]
    public IActionResult Wishlist()
    {
        return Redirect("/wishlist");
    }

    [HttpPost("account/address")]
    public IActionResult LegacySaveAddress([FromForm] string? address, [FromForm] string? csrf_token)
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");

        address = address?.Trim() ?? "";
        using var conn = Db.GetConnection();
        conn.Execute("UPDATE users SET address=@address WHERE id=@id", new { address, id = user.Id });

        // Update session
        user.Address = address;
        Auth.Login(HttpContext.Session, user);

        Flash("address_saved", "1");
        return Redirect("/account");
    }
}
