using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using ShopDotNet.Services;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class AuthController : BaseController
{
    private readonly SecurityService _security;

    public AuthController(SecurityService security) => _security = security;

    [HttpGet("login")]
    public IActionResult Login()
    {
        if (CurrentUser != null) return Redirect("/");
        ViewData["Title"] = "Sign In";
        ViewData["Errors"] = Array.Empty<string>();
        ViewData["Email"] = "";
        return View();
    }

    [HttpPost("login")]
    public IActionResult LoginPost([FromForm] string? email, [FromForm] string? password,
                                   [FromForm] string? csrf_token)
    {
        if (!ValidateCsrf(csrf_token))
            return BadRequest("Invalid CSRF token");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var errors = new List<string>();

        if (_security.IsRateLimited("login", ip))
        {
            errors.Add("Too many login attempts. Please try again later.");
            ViewData["Title"] = "Sign In";
            ViewData["Errors"] = errors;
            ViewData["Email"] = email ?? "";
            return View("Login");
        }

        email = email?.Trim().ToLower() ?? "";
        password = password ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errors.Add("Email and password are required.");
        }
        else
        {
            using var conn = Db.GetConnection();
            var user = conn.QueryFirstOrDefault<User>(
                "SELECT * FROM users WHERE email=@email", new { email });

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _security.RecordAttempt("login", ip);
                errors.Add("Invalid email or password.");
            }
            else
            {
                _security.ClearAttempts("login", ip);
                var session = new UserSession
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt,
                };
                AuthService.Login(HttpContext.Session, session);

                var redirect = HttpContext.Session.GetString("redirect_after_login") ?? "/";
                HttpContext.Session.Remove("redirect_after_login");
                return Redirect(redirect);
            }
        }

        ViewData["Title"] = "Sign In";
        ViewData["Errors"] = errors;
        ViewData["Email"] = email;
        return View("Login");
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (CurrentUser != null) return Redirect("/");
        ViewData["Title"] = "Create Account";
        ViewData["Errors"] = Array.Empty<string>();
        ViewData["Name"] = "";
        ViewData["Email"] = "";
        return View();
    }

    [HttpPost("register")]
    public IActionResult RegisterPost([FromForm] string? name, [FromForm] string? email,
                                      [FromForm] string? password, [FromForm] string? password2,
                                      [FromForm] string? csrf_token)
    {
        if (!ValidateCsrf(csrf_token))
            return BadRequest("Invalid CSRF token");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var errors = new List<string>();
        var minLen = int.Parse(Settings.Get("password_min_length"));

        if (_security.IsRateLimited("register", ip))
        {
            errors.Add("Too many registration attempts. Please try again later.");
            ViewData["Title"] = "Create Account";
            ViewData["Errors"] = errors;
            ViewData["Name"] = name ?? "";
            ViewData["Email"] = email ?? "";
            return View("Register");
        }

        name = name?.Trim() ?? "";
        email = email?.Trim().ToLower() ?? "";
        password = password ?? "";
        password2 = password2 ?? "";

        if (string.IsNullOrEmpty(name)) errors.Add("Name is required.");
        if (string.IsNullOrEmpty(email)) errors.Add("Email is required.");
        else if (!email.Contains('@') || !email.Contains('.')) errors.Add("Invalid email address.");
        if (password.Length < minLen) errors.Add($"Password must be at least {minLen} characters.");
        if (password != password2) errors.Add("Passwords do not match.");

        if (errors.Count == 0)
        {
            using var conn = Db.GetConnection();
            var existing = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM users WHERE email=@email", new { email });
            if (existing > 0)
            {
                errors.Add("An account with that email already exists.");
            }
            else
            {
                _security.ClearAttempts("register", ip);
                var hash = BCrypt.Net.BCrypt.HashPassword(password);
                conn.Execute(
                    "INSERT INTO users (name, email, password_hash, role) VALUES (@name, @email, @hash, 'customer')",
                    new { name, email, hash });
                var user = conn.QueryFirstOrDefault<User>(
                    "SELECT * FROM users WHERE email=@email", new { email })!;
                var session = new UserSession
                {
                    Id = user.Id, Name = user.Name, Email = user.Email,
                    Role = user.Role, CreatedAt = user.CreatedAt,
                };
                AuthService.Login(HttpContext.Session, session);
                return Redirect("/");
            }
        }
        else
        {
            _security.RecordAttempt("register", ip);
        }

        ViewData["Title"] = "Create Account";
        ViewData["Errors"] = errors;
        ViewData["Name"] = name;
        ViewData["Email"] = email;
        return View("Register");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        AuthService.Logout(HttpContext.Session);
        return Redirect("/");
    }
}
