using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/delivery")]
public class DeliveryController : AdminBaseController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        using var conn = Db.GetConnection();
        var options = conn.Query<DeliveryOption>("SELECT * FROM delivery_options ORDER BY price").ToList();

        ViewData["Title"] = "Delivery Options";
        ViewData["Active"] = "delivery";
        ViewData["Options"] = options;
        ViewData["FlashMsg"] = GetFlash("delivery_saved");
        return View("~/Views/Admin/Delivery/Index.cshtml");
    }

    [HttpGet("new")]
    public IActionResult New()
    {
        ViewData["Title"] = "Add Delivery Option";
        ViewData["Active"] = "delivery";
        ViewData["IsNew"] = true;
        ViewData["Option"] = new DeliveryOption { Active = true };
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Delivery/Edit.cshtml");
    }

    [HttpGet("edit")]
    public IActionResult Edit(int id)
    {
        using var conn = Db.GetConnection();
        var option = conn.QueryFirstOrDefault<DeliveryOption>("SELECT * FROM delivery_options WHERE id=@id", new { id });
        if (option == null) return NotFound();

        ViewData["Title"] = "Edit Delivery Option";
        ViewData["Active"] = "delivery";
        ViewData["IsNew"] = false;
        ViewData["Option"] = option;
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Delivery/Edit.cshtml");
    }

    [HttpPost("")]
    public IActionResult Save([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest();

        var errors = new List<string>();
        var isNew = !int.TryParse(form["id"], out var id) || id == 0;

        var name = form["name"].ToString().Trim();
        var active = form["active"].ToString() == "1";
        if (!decimal.TryParse(form["price"], out var price)) price = 0;
        if (!decimal.TryParse(form["min_order_total"], out var minOrderTotal)) minOrderTotal = 0;

        if (string.IsNullOrEmpty(name)) errors.Add("Name is required.");

        if (errors.Count > 0)
        {
            var opt = new DeliveryOption { Id = id, Name = name, Price = price,
                MinOrderTotal = minOrderTotal, Active = active };
            ViewData["Title"] = isNew ? "Add Delivery Option" : "Edit Delivery Option";
            ViewData["Active"] = "delivery";
            ViewData["IsNew"] = isNew;
            ViewData["Option"] = opt;
            ViewData["Errors"] = errors;
            return View("~/Views/Admin/Delivery/Edit.cshtml");
        }

        using var conn = Db.GetConnection();
        if (isNew)
            conn.Execute(
                "INSERT INTO delivery_options (name, price, active, min_order_total) VALUES (@name, @price, @active, @minOrderTotal)",
                new { name, price, active = active ? 1 : 0, minOrderTotal });
        else
            conn.Execute(
                "UPDATE delivery_options SET name=@name, price=@price, active=@active, min_order_total=@minOrderTotal WHERE id=@id",
                new { name, price, active = active ? 1 : 0, minOrderTotal, id });

        Flash("delivery_saved", isNew ? "Delivery option created." : "Delivery option updated.");
        return Redirect("/admin/delivery");
    }

    [HttpGet("delete")]
    public IActionResult Delete(int id)
    {
        using var conn = Db.GetConnection();
        conn.Execute("DELETE FROM delivery_options WHERE id=@id", new { id });
        Flash("delivery_saved", "Delivery option deleted.");
        return Redirect("/admin/delivery");
    }
}
