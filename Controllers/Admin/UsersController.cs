using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/users")]
public class UsersController : AdminBaseController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        using var conn = Db.GetConnection();
        var users = conn.Query<User>(@"
            SELECT u.*, COUNT(o.id) as OrderCount
            FROM users u LEFT JOIN orders o ON o.user_id=u.id
            GROUP BY u.id ORDER BY u.created_at DESC").ToList();

        ViewData["Title"] = "Users";
        ViewData["Active"] = "users";
        ViewData["Users"] = users;
        ViewData["FlashMsg"] = GetFlash("user_saved");
        ViewData["FlashErr"] = GetFlash("user_error");
        return View("~/Views/Admin/Users/Index.cshtml");
    }

    [HttpGet("new")]
    public IActionResult New()
    {
        ViewData["Title"] = "Add User";
        ViewData["Active"] = "users";
        ViewData["IsNew"] = true;
        ViewData["UserId"] = 0;
        ViewData["User"] = new User();
        ViewData["PasswordMinLen"] = int.Parse(Settings.Get("password_min_length"));
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Users/Edit.cshtml");
    }

    [HttpGet("edit")]
    public IActionResult Edit(int id)
    {
        using var conn = Db.GetConnection();
        var user = conn.QueryFirstOrDefault<User>("SELECT * FROM users WHERE id=@id", new { id });
        if (user == null) return NotFound();

        ViewData["Title"] = $"Edit: {user.Name}";
        ViewData["Active"] = "users";
        ViewData["IsNew"] = false;
        ViewData["UserId"] = id;
        ViewData["User"] = user;
        ViewData["PasswordMinLen"] = int.Parse(Settings.Get("password_min_length"));
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Users/Edit.cshtml");
    }

    [HttpPost("")]
    public IActionResult Save([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest();

        var errors = new List<string>();
        var isNew = !int.TryParse(form["id"], out var id) || id == 0;
        var minLen = int.Parse(Settings.Get("password_min_length"));

        var name = form["name"].ToString().Trim();
        var email = form["email"].ToString().Trim().ToLower();
        var role = form["role"].ToString();
        var address = form["address"].ToString().Trim();
        var password = form["password"].ToString();

        if (string.IsNullOrEmpty(name)) errors.Add("Name is required.");
        if (string.IsNullOrEmpty(email)) errors.Add("Email is required.");
        if (!new[] { "admin", "customer" }.Contains(role)) role = "customer";
        if (isNew && password.Length < minLen) errors.Add($"Password must be at least {minLen} characters.");
        if (!isNew && !string.IsNullOrEmpty(password) && password.Length < minLen)
            errors.Add($"Password must be at least {minLen} characters.");

        if (errors.Count > 0)
        {
            var u = new User { Id = id, Name = name, Email = email, Role = role, Address = address };
            ViewData["Title"] = isNew ? "Add User" : $"Edit: {name}";
            ViewData["Active"] = "users";
            ViewData["IsNew"] = isNew;
            ViewData["UserId"] = id;
            ViewData["User"] = u;
            ViewData["PasswordMinLen"] = minLen;
            ViewData["Errors"] = errors;
            return View("~/Views/Admin/Users/Edit.cshtml");
        }

        using var conn = Db.GetConnection();
        if (isNew)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            conn.Execute(
                "INSERT INTO users (name, email, password_hash, role, address) VALUES (@name, @email, @hash, @role, @address)",
                new { name, email, hash, role, address });
        }
        else
        {
            if (!string.IsNullOrEmpty(password))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(password);
                conn.Execute(
                    "UPDATE users SET name=@name, email=@email, password_hash=@hash, role=@role, address=@address WHERE id=@id",
                    new { name, email, hash, role, address, id });
            }
            else
            {
                conn.Execute(
                    "UPDATE users SET name=@name, email=@email, role=@role, address=@address WHERE id=@id",
                    new { name, email, role, address, id });
            }
        }

        Flash("user_saved", isNew ? "User created." : "User updated.");
        return Redirect("/admin/users");
    }

    [HttpGet("delete")]
    public IActionResult Delete(int id)
    {
        var me = CurrentUser;
        if (me?.Id == id)
        {
            Flash("user_error", "You cannot delete your own account.");
            return Redirect("/admin/users");
        }
        using var conn = Db.GetConnection();
        conn.Execute("DELETE FROM users WHERE id=@id", new { id });
        Flash("user_saved", "User deleted.");
        return Redirect("/admin/users");
    }
}
