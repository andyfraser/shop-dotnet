using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers;

[Route("")]
public class StorefrontController : BaseController
{
    [HttpGet("")]
    [HttpGet("home")]
    public IActionResult Index()
    {
        using var conn = Db.GetConnection();
        var featured = conn.Query<Product>(@"
            SELECT p.*, c.name as CatName, c.slug as CatSlug
            FROM products p
            LEFT JOIN categories c ON p.category_id = c.id
            WHERE p.active = 1 AND (p.featured = 1 OR 1=1)
            ORDER BY p.featured DESC, p.created_at DESC
            LIMIT 8").ToList();

        ViewData["Title"] = "Home";
        ViewData["FeaturedProducts"] = featured;
        return View();
    }

    [HttpGet("search")]
    public IActionResult Search(string? q, string sort = "name", string per_page = "12", int page = 1)
    {
        var query = q?.Trim() ?? "";
        var validSorts = new Dictionary<string, string>
        {
            ["name"] = "p.name ASC",
            ["featured"] = "p.featured DESC, p.name ASC",
            ["price_asc"] = "p.price ASC",
            ["price_desc"] = "p.price DESC",
        };
        var orderBy = validSorts.TryGetValue(sort, out var s) ? s : "p.name ASC";

        using var conn = Db.GetConnection();
        List<Product> products;
        int totalProducts;

        if (!string.IsNullOrEmpty(query))
        {
            var likeQ = $"%{query}%";
            totalProducts = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM products p WHERE p.active=1 AND (p.name LIKE @q OR p.description LIKE @q)",
                new { q = likeQ });

            int limit, offset = 0;
            if (per_page == "all") { limit = totalProducts + 1; }
            else { limit = int.TryParse(per_page, out var pp) ? pp : 12; offset = (page - 1) * limit; }

            products = conn.Query<Product>($@"
                SELECT p.*, c.name as CatName
                FROM products p
                LEFT JOIN categories c ON p.category_id = c.id
                WHERE p.active=1 AND (p.name LIKE @q OR p.description LIKE @q)
                ORDER BY {orderBy} LIMIT @limit OFFSET @offset",
                new { q = likeQ, limit, offset }).ToList();
        }
        else
        {
            products = new();
            totalProducts = 0;
        }

        int perPageInt = per_page == "all" ? totalProducts : (int.TryParse(per_page, out var pp2) ? pp2 : 12);
        int totalPages = perPageInt > 0 ? (int)Math.Ceiling((double)totalProducts / perPageInt) : 1;

        ViewData["Title"] = string.IsNullOrEmpty(query) ? "Search" : $"Results for \"{query}\"";
        ViewData["Query"] = query;
        ViewData["Sort"] = sort;
        ViewData["PerPageParam"] = per_page;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalProducts"] = totalProducts;
        ViewData["Products"] = products;
        return View();
    }

    [HttpGet("category/{slug}")]
    public IActionResult Category(string slug, string sort = "name", string per_page = "12", int page = 1)
    {
        using var conn = Db.GetConnection();
        var category = conn.QueryFirstOrDefault<Category>(
            "SELECT * FROM categories WHERE slug=@slug", new { slug });
        if (category == null) return NotFound();

        // Get all descendant category IDs
        var allCats = conn.Query<Category>("SELECT * FROM categories").ToDictionary(c => c.Id);
        var ids = GetDescendantIds(allCats, category.Id);
        ids.Add(category.Id);
        var idList = string.Join(",", ids);

        var subcategories = conn.Query<Category>(
            "SELECT * FROM categories WHERE parent_id=@id ORDER BY name", new { id = category.Id }).ToList();

        var validSorts = new Dictionary<string, string>
        {
            ["name"] = "p.name ASC",
            ["featured"] = "p.featured DESC, p.name ASC",
            ["price_asc"] = "p.price ASC",
            ["price_desc"] = "p.price DESC",
        };
        var orderBy = validSorts.TryGetValue(sort, out var s) ? s : "p.name ASC";

        var totalProducts = conn.ExecuteScalar<int>(
            $"SELECT COUNT(*) FROM products p WHERE p.active=1 AND p.category_id IN ({idList})");

        int limit, offset = 0;
        if (per_page == "all") { limit = totalProducts + 1; }
        else { limit = int.TryParse(per_page, out var pp) ? pp : 12; offset = (page - 1) * limit; }

        var products = conn.Query<Product>($@"
            SELECT p.*, c.name as CatName
            FROM products p LEFT JOIN categories c ON p.category_id=c.id
            WHERE p.active=1 AND p.category_id IN ({idList})
            ORDER BY {orderBy} LIMIT @limit OFFSET @offset",
            new { limit, offset }).ToList();

        int perPageInt = per_page == "all" ? totalProducts : (int.TryParse(per_page, out var pp2) ? pp2 : 12);
        int totalPages = perPageInt > 0 ? (int)Math.Ceiling((double)totalProducts / perPageInt) : 1;

        var breadcrumb = GetBreadcrumb(allCats, category.Id);

        ViewData["Title"] = category.Name;
        ViewData["Category"] = category;
        ViewData["Subcategories"] = subcategories;
        ViewData["Products"] = products;
        ViewData["Breadcrumb"] = breadcrumb;
        ViewData["Sort"] = sort;
        ViewData["PerPageParam"] = per_page;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalProducts"] = totalProducts;
        return View();
    }

    [HttpGet("product/{slug}")]
    public IActionResult Product(string slug)
    {
        using var conn = Db.GetConnection();
        var product = conn.QueryFirstOrDefault<Product>(@"
            SELECT p.*, c.name as CatName, c.slug as CatSlug
            FROM products p LEFT JOIN categories c ON p.category_id=c.id
            WHERE p.slug=@slug AND p.active=1", new { slug });
        if (product == null) return NotFound();

        var allCats = conn.Query<Category>("SELECT * FROM categories").ToDictionary(c => c.Id);
        var breadcrumb = product.CategoryId.HasValue
            ? GetBreadcrumb(allCats, product.CategoryId.Value)
            : new List<Category>();

        var related = product.CategoryId.HasValue
            ? conn.Query<Product>(@"
                SELECT * FROM products WHERE active=1 AND category_id=@catId AND id!=@id
                ORDER BY RANDOM() LIMIT 4",
                new { catId = product.CategoryId, id = product.Id }).ToList()
            : new List<Product>();

        var lowStock = int.Parse(Settings.Get("low_stock_threshold"));
        var flashSuccess = GetFlash("cart_success");

        ViewData["Title"] = product.Name;
        ViewData["Product"] = product;
        ViewData["Breadcrumb"] = breadcrumb;
        ViewData["RelatedProducts"] = related;
        ViewData["LowStockThreshold"] = lowStock;
        ViewData["FlashSuccess"] = flashSuccess;
        return View();
    }

    [HttpGet("favicon.ico")]
    [HttpGet("apple-touch-icon.png")]
    [HttpGet("apple-touch-icon-precomposed.png")]
    public IActionResult HandleIcon() => NoContent();

    private List<int> GetDescendantIds(Dictionary<int, Category> all, int parentId)
    {
        var result = new List<int>();
        foreach (var c in all.Values.Where(c => c.ParentId == parentId))
        {
            result.Add(c.Id);
            result.AddRange(GetDescendantIds(all, c.Id));
        }
        return result;
    }

    private List<Category> GetBreadcrumb(Dictionary<int, Category> all, int categoryId)
    {
        var crumbs = new List<Category>();
        Category? current = all.TryGetValue(categoryId, out var c) ? c : null;
        while (current != null)
        {
            crumbs.Insert(0, current);
            current = current.ParentId.HasValue && all.TryGetValue(current.ParentId.Value, out var p) ? p : null;
        }
        return crumbs;
    }
}
