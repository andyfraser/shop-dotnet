using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin")]
public class DashboardController : AdminBaseController
{
    [HttpGet("")]
    [HttpGet("dashboard")]
    public IActionResult Index()
    {
        using var conn = Db.GetConnection();
        var lowThreshold = int.Parse(Settings.Get("low_stock_threshold"));

        var stats = new
        {
            products = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM products WHERE active=1"),
            customers = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE role='customer'"),
            orders = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM orders"),
            revenue = conn.ExecuteScalar<decimal>("SELECT COALESCE(SUM(total),0) FROM orders WHERE status!='cancelled'"),
        };

        var recentOrders = conn.Query<Models.Order>(@"
            SELECT o.*, u.name as UserName
            FROM orders o LEFT JOIN users u ON o.user_id=u.id
            ORDER BY o.created_at DESC LIMIT 10").ToList();

        var lowStock = conn.Query<Models.Product>(
            "SELECT * FROM products WHERE active=1 AND stock<=@t ORDER BY stock ASC LIMIT 10",
            new { t = lowThreshold }).ToList();

        ViewData["Title"] = "Dashboard";
        ViewData["Active"] = "dashboard";
        ViewData["Stats"] = stats;
        ViewData["RecentOrders"] = recentOrders;
        ViewData["LowStock"] = lowStock;
        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}
