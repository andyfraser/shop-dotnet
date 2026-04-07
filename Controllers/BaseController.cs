using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopDotNet.Models;
using ShopDotNet.Services;
using Dapper;

namespace ShopDotNet.Controllers;

public abstract class BaseController : Controller
{
    protected DatabaseService Db => HttpContext.RequestServices.GetRequiredService<DatabaseService>();
    protected SettingsService Settings => HttpContext.RequestServices.GetRequiredService<SettingsService>();
    protected CartService Cart => HttpContext.RequestServices.GetRequiredService<CartService>();

    protected UserSession? CurrentUser => AuthService.GetCurrentUser(HttpContext.Session);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var user = CurrentUser;
        var siteName = Settings.Get("site_name");
        var currency = Settings.Get("currency_symbol");
        var cartCount = Cart.GetCount(HttpContext.Session);
        var navTree = GetNavTree();

        ViewData["CurrentUser"] = user;
        ViewData["CartCount"] = cartCount;
        ViewData["NavTree"] = navTree;
        ViewData["SiteName"] = siteName;
        ViewData["CurrencySymbol"] = currency;
        ViewData["CsrfToken"] = SecurityService.GetOrCreateCsrfToken(HttpContext.Session);
    }

    protected List<Category> GetNavTree()
    {
        using var conn = Db.GetConnection();
        var all = conn.Query<Category>("SELECT * FROM categories ORDER BY name").ToList();
        var map = all.ToDictionary(c => c.Id, c => c);
        var tree = new List<Category>();
        foreach (var c in all)
        {
            if (c.ParentId.HasValue && map.TryGetValue(c.ParentId.Value, out var parent))
                parent.Children.Add(c);
            else
                tree.Add(c);
        }
        return tree;
    }

    protected string Money(decimal value)
    {
        var sym = Settings.Get("currency_symbol");
        return $"{sym}{value:F2}";
    }

    protected void Flash(string key, string message)
    {
        TempData[$"flash_{key}"] = message;
    }

    protected string? GetFlash(string key)
    {
        return TempData[$"flash_{key}"] as string;
    }

    protected bool ValidateCsrf(string? token)
    {
        return SecurityService.ValidateCsrf(HttpContext.Session, token);
    }

    protected bool IsNewProduct(string createdAt)
    {
        if (DateTime.TryParse(createdAt, out var dt))
            return (DateTime.UtcNow - dt).TotalDays < 7;
        return false;
    }

    protected string SlugPath(string path)
    {
        return string.Join("-", System.Text.RegularExpressions.Regex.Split(path.ToLower().Trim(), @"[^a-z0-9]+"))
               .Trim('-');
    }
}
