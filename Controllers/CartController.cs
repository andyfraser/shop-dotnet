using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class CartController : BaseController
{
    [HttpGet("cart")]
    public IActionResult Index()
    {
        var items = Cart.GetItems(HttpContext.Session);
        var total = items.Sum(i => i.Subtotal);
        ViewData["Title"] = "Shopping Cart";
        ViewData["Items"] = items;
        ViewData["Total"] = total;
        ViewData["FlashSuccess"] = GetFlash("cart_success");
        return View();
    }

    [HttpPost("cart")]
    public IActionResult Update([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken))
            return Json(new { ok = false, message = "Invalid CSRF token" });

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!string.IsNullOrEmpty(form["remove"]))
        {
            if (int.TryParse(form["remove"], out var removeId))
                Cart.Remove(HttpContext.Session, removeId);

            var items = Cart.GetItems(HttpContext.Session);
            var total = items.Sum(i => i.Subtotal);
            var count = Cart.GetCount(HttpContext.Session);

            if (isAjax)
                return Json(new { ok = true, cart_count = count, total = Money(total) });

            Flash("cart_success", "Item removed.");
            return Redirect("/cart");
        }

        if (!string.IsNullOrEmpty(form["update"]))
        {
            var updates = new Dictionary<int, int>();
            foreach (var key in form.Keys.Where(k => k.StartsWith("qty[")))
            {
                var idStr = key.TrimStart('q','t','y','[').TrimEnd(']');
                if (int.TryParse(idStr, out var pid) && int.TryParse(form[key], out var qty))
                    updates[pid] = qty;
            }
            Cart.Update(HttpContext.Session, updates);

            var items = Cart.GetItems(HttpContext.Session);
            var total = items.Sum(i => i.Subtotal);
            var count = Cart.GetCount(HttpContext.Session);

            if (isAjax)
            {
                var itemData = items.Select(i => new {
                    id = i.Id,
                    subtotal = Money(i.Subtotal)
                });
                return Json(new { ok = true, cart_count = count, total = Money(total),
                    message = "Cart updated.", items = itemData });
            }

            Flash("cart_success", "Cart updated.");
            return Redirect("/cart");
        }

        return Redirect("/cart");
    }

    [HttpPost("product/{slug}")]
    public IActionResult Add(string slug, [FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken))
            return Json(new { ok = false, message = "Invalid CSRF token" });

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!int.TryParse(form["product_id"], out var productId) ||
            !int.TryParse(form["qty"], out var qty) || qty < 1)
        {
            if (isAjax) return Json(new { ok = false, message = "Invalid request." });
            return Redirect($"/product/{slug}");
        }

        using var conn = Db.GetConnection();
        var product = conn.QueryFirstOrDefault<Product>(
            "SELECT * FROM products WHERE id=@id AND active=1", new { id = productId });

        if (product == null || product.Stock < 1)
        {
            if (isAjax) return Json(new { ok = false, message = "Product unavailable." });
            return Redirect($"/product/{slug}");
        }

        Cart.Add(HttpContext.Session, productId, qty);
        var count = Cart.GetCount(HttpContext.Session);
        var message = $"Added {qty} × {product.Name} to your cart.";

        if (isAjax)
            return Json(new { ok = true, cart_count = count, message });

        Flash("cart_success", message);
        return Redirect($"/product/{slug}");
    }
}
