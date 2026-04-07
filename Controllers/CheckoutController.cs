using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class CheckoutController : BaseController
{
    [HttpGet("checkout")]
    public IActionResult Index()
    {
        var items = Cart.GetItems(HttpContext.Session);
        if (items.Count == 0) return Redirect("/cart");

        var total = items.Sum(i => i.Subtotal);
        using var conn = Db.GetConnection();
        var deliveryOptions = conn.Query<DeliveryOption>(
            "SELECT * FROM delivery_options WHERE active=1 AND min_order_total<=@total ORDER BY price",
            new { total }).ToList();

        var user = CurrentUser;
        ViewData["Title"] = "Checkout";
        ViewData["Items"] = items;
        ViewData["Total"] = total;
        ViewData["DeliveryOptions"] = deliveryOptions;
        ViewData["IsGuest"] = user == null;
        ViewData["Name"] = user?.Name ?? "";
        ViewData["Email"] = user?.Email ?? "";
        ViewData["Address"] = user?.Address ?? "";
        ViewData["Notes"] = "";
        ViewData["DeliveryId"] = "";
        ViewData["Errors"] = Array.Empty<string>();
        return View();
    }

    [HttpPost("checkout")]
    public IActionResult Process([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken))
            return BadRequest("Invalid CSRF token");

        var items = Cart.GetItems(HttpContext.Session);
        if (items.Count == 0) return Redirect("/cart");

        var total = items.Sum(i => i.Subtotal);
        using var conn = Db.GetConnection();
        var deliveryOptions = conn.Query<DeliveryOption>(
            "SELECT * FROM delivery_options WHERE active=1 AND min_order_total<=@total ORDER BY price",
            new { total }).ToList();

        var name = form["name"].ToString().Trim();
        var email = form["email"].ToString().Trim();
        var address = form["address"].ToString().Trim();
        var notes = form["notes"].ToString().Trim();
        var deliveryIdStr = form["delivery_option_id"].ToString();
        var errors = new List<string>();

        if (string.IsNullOrEmpty(name)) errors.Add("Name is required.");
        if (string.IsNullOrEmpty(email)) errors.Add("Email is required.");
        if (string.IsNullOrEmpty(address)) errors.Add("Shipping address is required.");

        DeliveryOption? delivery = null;
        if (!int.TryParse(deliveryIdStr, out var deliveryId))
            errors.Add("Please select a delivery method.");
        else
        {
            delivery = deliveryOptions.FirstOrDefault(d => d.Id == deliveryId);
            if (delivery == null) errors.Add("Invalid delivery method.");
        }

        if (errors.Count > 0)
        {
            ViewData["Title"] = "Checkout";
            ViewData["Items"] = items;
            ViewData["Total"] = total;
            ViewData["DeliveryOptions"] = deliveryOptions;
            ViewData["IsGuest"] = CurrentUser == null;
            ViewData["Name"] = name;
            ViewData["Email"] = email;
            ViewData["Address"] = address;
            ViewData["Notes"] = notes;
            ViewData["DeliveryId"] = deliveryIdStr;
            ViewData["Errors"] = errors;
            return View("Index");
        }

        var orderTotal = total + (delivery?.Price ?? 0);
        var user = CurrentUser;

        conn.Execute(@"INSERT INTO orders
            (user_id, status, total, shipping_address, notes, delivery_method, delivery_cost, customer_email, customer_name)
            VALUES (@userId, 'pending', @total, @address, @notes, @deliveryMethod, @deliveryCost, @email, @name)",
            new {
                userId = user?.Id as object ?? DBNull.Value,
                total = orderTotal,
                address,
                notes,
                deliveryMethod = delivery!.Name,
                deliveryCost = delivery.Price,
                email,
                name,
            });

        var orderId = (int)conn.ExecuteScalar<long>("SELECT last_insert_rowid()");

        // Insert order items and decrement stock
        foreach (var item in items)
        {
            conn.Execute(
                "INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES (@orderId, @productId, @qty, @price)",
                new { orderId, productId = item.Id, qty = item.Qty, price = item.Price });
            conn.Execute(
                "UPDATE products SET stock = MAX(0, stock - @qty) WHERE id=@id",
                new { qty = item.Qty, id = item.Id });
        }

        Cart.Clear(HttpContext.Session);
        HttpContext.Session.SetInt32("last_order_id", orderId);

        return Redirect($"/order/confirm?id={orderId}");
    }

    [HttpGet("order/confirm")]
    public IActionResult Confirm(int? id)
    {
        if (id == null) return Redirect("/");

        var user = CurrentUser;
        var lastOrderId = HttpContext.Session.GetInt32("last_order_id");

        // Allow access if: logged-in user who placed order, OR guest who just placed this order
        using var conn = Db.GetConnection();
        var order = conn.QueryFirstOrDefault<Order>(@"
            SELECT o.*, u.name as UserName, u.email as UserEmail
            FROM orders o LEFT JOIN users u ON o.user_id = u.id
            WHERE o.id=@id", new { id });

        if (order == null) return NotFound();

        // Access check
        bool canView = (user != null && order.UserId == user.Id)
                    || lastOrderId == id
                    || (user != null && user.IsAdmin);
        if (!canView) return Redirect("/login");

        var orderItems = conn.Query<OrderItem>(@"
            SELECT oi.*, p.name as Name, p.slug as Slug
            FROM order_items oi
            LEFT JOIN products p ON oi.product_id = p.id
            WHERE oi.order_id=@id", new { id }).ToList();

        ViewData["Title"] = $"Order #{id:D6} Confirmed";
        ViewData["Order"] = order;
        ViewData["OrderItems"] = orderItems;
        return View();
    }
}
