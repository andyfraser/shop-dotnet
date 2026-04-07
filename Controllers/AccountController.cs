using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using ShopDotNet.Services;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class AccountController : BaseController
{
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
            ORDER BY o.created_at DESC",
            new { userId = user.Id }).ToList();

        ViewData["Title"] = "My Account";
        ViewData["Orders"] = orders;
        return View();
    }

    [HttpPost("account/address")]
    public IActionResult SaveAddress([FromForm] string? address, [FromForm] string? csrf_token)
    {
        var user = CurrentUser;
        if (user == null) return Redirect("/login");
        if (!ValidateCsrf(csrf_token)) return BadRequest("Invalid CSRF token");

        address = address?.Trim() ?? "";
        using var conn = Db.GetConnection();
        conn.Execute("UPDATE users SET address=@address WHERE id=@id", new { address, id = user.Id });

        // Update session
        user.Address = address;
        AuthService.Login(HttpContext.Session, user);

        Flash("address_saved", "1");
        return Redirect("/account");
    }
}
