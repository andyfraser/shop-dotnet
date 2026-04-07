using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/orders")]
public class OrdersController : AdminBaseController
{
    [HttpGet("")]
    public IActionResult Index(string? status)
    {
        using var conn = Db.GetConnection();
        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        var filter = validStatuses.Contains(status) ? status : "";

        List<Order> orders;
        if (!string.IsNullOrEmpty(filter))
        {
            orders = conn.Query<Order>(@"
                SELECT o.*, u.name as UserName FROM orders o
                LEFT JOIN users u ON o.user_id=u.id
                WHERE o.status=@status ORDER BY o.created_at DESC",
                new { status = filter }).ToList();
        }
        else
        {
            orders = conn.Query<Order>(@"
                SELECT o.*, u.name as UserName FROM orders o
                LEFT JOIN users u ON o.user_id=u.id
                ORDER BY o.created_at DESC").ToList();
        }

        ViewData["Title"] = "Orders";
        ViewData["Active"] = "orders";
        ViewData["Orders"] = orders;
        ViewData["Filter"] = filter;
        return View("~/Views/Admin/Orders/Index.cshtml");
    }

    [HttpGet("detail")]
    public IActionResult Detail(int id)
    {
        using var conn = Db.GetConnection();
        var order = conn.QueryFirstOrDefault<Order>(@"
            SELECT o.*, u.name as UserName, u.email as UserEmail
            FROM orders o LEFT JOIN users u ON o.user_id=u.id
            WHERE o.id=@id", new { id });
        if (order == null) return NotFound();

        var items = conn.Query<OrderItem>(@"
            SELECT oi.*, p.name as ProductName, p.slug as Slug
            FROM order_items oi LEFT JOIN products p ON oi.product_id=p.id
            WHERE oi.order_id=@id", new { id }).ToList();

        ViewData["Title"] = $"Order #{id:D6}";
        ViewData["Active"] = "orders";
        ViewData["Order"] = order;
        ViewData["OrderItems"] = items;
        ViewData["FlashMsg"] = GetFlash("order_updated");
        return View("~/Views/Admin/Orders/Detail.cshtml");
    }

    [HttpPost("detail")]
    public IActionResult UpdateStatus([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest();

        if (!int.TryParse(form["id"], out var id)) return BadRequest();
        var status = form["status"].ToString();
        var valid = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!valid.Contains(status)) return BadRequest();

        using var conn = Db.GetConnection();
        conn.Execute("UPDATE orders SET status=@status WHERE id=@id", new { status, id });

        Flash("order_updated", "Order status updated.");
        return Redirect($"/admin/orders/detail?id={id}");
    }
}
