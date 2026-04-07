using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/categories")]
public class CategoriesController : AdminBaseController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        using var conn = Db.GetConnection();
        var categories = conn.Query<Category>(@"
            SELECT c.*, p.name as ParentName,
                   (SELECT COUNT(*) FROM products WHERE category_id=c.id) as ProductCount
            FROM categories c LEFT JOIN categories p ON c.parent_id=p.id
            ORDER BY COALESCE(p.name, c.name), c.parent_id IS NOT NULL, c.name").ToList();

        ViewData["Title"] = "Categories";
        ViewData["Active"] = "categories";
        ViewData["Categories"] = categories;
        ViewData["FlashMsg"] = GetFlash("category_saved");
        return View("~/Views/Admin/Categories/Index.cshtml");
    }

    [HttpGet("new")]
    public IActionResult New()
    {
        ViewData["Title"] = "Add Category";
        ViewData["Active"] = "categories";
        ViewData["IsNew"] = true;
        ViewData["CategoryId"] = 0;
        ViewData["Category"] = new Category();
        ViewData["AllCategories"] = GetAll();
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Categories/Edit.cshtml");
    }

    [HttpGet("edit")]
    public IActionResult Edit(int id)
    {
        using var conn = Db.GetConnection();
        var category = conn.QueryFirstOrDefault<Category>("SELECT * FROM categories WHERE id=@id", new { id });
        if (category == null) return NotFound();

        ViewData["Title"] = "Edit Category";
        ViewData["Active"] = "categories";
        ViewData["IsNew"] = false;
        ViewData["CategoryId"] = id;
        ViewData["Category"] = category;
        ViewData["AllCategories"] = GetAll();
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Categories/Edit.cshtml");
    }

    [HttpPost("")]
    public IActionResult Save([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest();

        var errors = new List<string>();
        var isNew = !int.TryParse(form["id"], out var id) || id == 0;

        var name = form["name"].ToString().Trim();
        var parentIdStr = form["parent_id"].ToString();
        var icon = form["icon"].ToString().Trim();
        var description = form["description"].ToString().Trim();

        if (string.IsNullOrEmpty(name)) errors.Add("Name is required.");
        int? parentId = int.TryParse(parentIdStr, out var pid) ? pid : null;

        if (errors.Count > 0)
        {
            var category = new Category { Id = id, Name = name, ParentId = parentId,
                Icon = icon, Description = description };
            ViewData["Title"] = isNew ? "Add Category" : "Edit Category";
            ViewData["Active"] = "categories";
            ViewData["IsNew"] = isNew;
            ViewData["CategoryId"] = id;
            ViewData["Category"] = category;
            ViewData["AllCategories"] = GetAll();
            ViewData["Errors"] = errors;
            return View("~/Views/Admin/Categories/Edit.cshtml");
        }

        using var conn = Db.GetConnection();
        if (isNew)
        {
            var slug = GenerateSlug(name);
            slug = EnsureUniqueSlug(conn, slug, 0);
            conn.Execute(
                "INSERT INTO categories (name, slug, parent_id, icon, description) VALUES (@name, @slug, @parentId, @icon, @desc)",
                new { name, slug, parentId = parentId as object ?? DBNull.Value, icon, desc = description });
        }
        else
        {
            conn.Execute(
                "UPDATE categories SET name=@name, parent_id=@parentId, icon=@icon, description=@desc WHERE id=@id",
                new { name, parentId = parentId as object ?? DBNull.Value, icon, desc = description, id });
        }

        Flash("category_saved", isNew ? "Category created." : "Category updated.");
        return Redirect("/admin/categories");
    }

    [HttpGet("delete")]
    public IActionResult Delete(int id)
    {
        using var conn = Db.GetConnection();
        conn.Execute("DELETE FROM categories WHERE id=@id", new { id });
        Flash("category_saved", "Category deleted.");
        return Redirect("/admin/categories");
    }

    private List<Category> GetAll()
    {
        using var conn = Db.GetConnection();
        return conn.Query<Category>("SELECT * FROM categories ORDER BY name").ToList();
    }

    private static string GenerateSlug(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');

    private static string EnsureUniqueSlug(Microsoft.Data.Sqlite.SqliteConnection conn, string slug, int excludeId)
    {
        var candidate = slug;
        var n = 1;
        while (conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM categories WHERE slug=@s AND id!=@id",
            new { s = candidate, id = excludeId }) > 0)
        { candidate = $"{slug}-{n++}"; }
        return candidate;
    }
}
