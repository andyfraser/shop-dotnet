using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopDotNet.Models;
using ShopDotNet.Services;
using Dapper;

namespace ShopDotNet.Controllers;

public abstract class BaseController : Controller
{
    protected readonly IDatabaseService Db;
    protected readonly ISettingsService Settings;
    protected readonly ICartService Cart;
    protected readonly IAuthService Auth;
    protected readonly ISecurityService Security;

    protected BaseController(
        IDatabaseService db,
        ISettingsService settings,
        ICartService cart,
        IAuthService auth,
        ISecurityService security)
    {
        Db = db;
        Settings = settings;
        Cart = cart;
        Auth = auth;
        Security = security;
    }

    protected UserSession? CurrentUser => Auth.GetCurrentUser(HttpContext.Session);

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
        ViewData["CsrfToken"] = Security.GetOrCreateCsrfToken(HttpContext.Session);
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
        return Security.ValidateCsrf(HttpContext.Session, token);
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
