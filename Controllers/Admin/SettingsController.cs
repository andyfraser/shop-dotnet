using Microsoft.AspNetCore.Mvc;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/settings")]
public class SettingsController : AdminBaseController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Settings";
        ViewData["Active"] = "settings";
        ViewData["Settings"] = Settings.GetAll();
        ViewData["FlashMsg"] = GetFlash("settings_saved");
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Settings/Index.cshtml");
    }

    [HttpPost("")]
    public IActionResult Save([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest();

        var errors = new List<string>();
        var siteName = form["site_name"].ToString().Trim();
        var currencySymbol = form["currency_symbol"].ToString().Trim();

        if (string.IsNullOrEmpty(siteName)) errors.Add("Site name is required.");
        if (string.IsNullOrEmpty(currencySymbol)) errors.Add("Currency symbol is required.");

        var keys = new[] { "site_name", "currency_symbol", "password_min_length",
            "login_max_attempts", "login_window_minutes",
            "register_max_attempts", "register_window_minutes", "low_stock_threshold" };

        foreach (var key in keys)
        {
            if (!new[] { "site_name", "currency_symbol" }.Contains(key))
            {
                if (!int.TryParse(form[key].ToString(), out var v) || v < 1)
                    errors.Add($"{key.Replace('_', ' ')} must be a positive number.");
            }
        }

        if (errors.Count > 0)
        {
            ViewData["Title"] = "Settings";
            ViewData["Active"] = "settings";
            ViewData["Settings"] = Settings.GetAll();
            ViewData["Errors"] = errors;
            ViewData["FlashMsg"] = null;
            return View("~/Views/Admin/Settings/Index.cshtml");
        }

        var values = keys.ToDictionary(k => k, k => form[k].ToString().Trim());
        Settings.Save(values);

        Flash("settings_saved", "Settings saved.");
        return Redirect("/admin/settings");
    }
}
