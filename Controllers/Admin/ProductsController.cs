using Microsoft.AspNetCore.Mvc;
using ShopDotNet.Models;
using Dapper;

namespace ShopDotNet.Controllers.Admin;

[Route("admin/products")]
public class ProductsController : AdminBaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public ProductsController(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
    }

    [HttpGet("")]
    public IActionResult Index(string? search)
    {
        using var conn = Db.GetConnection();
        List<Product> products;
        if (!string.IsNullOrEmpty(search))
        {
            products = conn.Query<Product>(@"
                SELECT p.*, c.name as CatName FROM products p
                LEFT JOIN categories c ON p.category_id=c.id
                WHERE p.name LIKE @q ORDER BY p.name",
                new { q = $"%{search}%" }).ToList();
        }
        else
        {
            products = conn.Query<Product>(@"
                SELECT p.*, c.name as CatName FROM products p
                LEFT JOIN categories c ON p.category_id=c.id
                ORDER BY p.name").ToList();
        }

        ViewData["Title"] = "Products";
        ViewData["Active"] = "products";
        ViewData["Products"] = products;
        ViewData["Search"] = search ?? "";
        ViewData["FlashMsg"] = GetFlash("product_saved");
        ViewData["LowStockThreshold"] = int.Parse(Settings.Get("low_stock_threshold"));
        return View("~/Views/Admin/Products/Index.cshtml");
    }

    [HttpGet("new")]
    public IActionResult New()
    {
        ViewData["Title"] = "Add Product";
        ViewData["Active"] = "products";
        ViewData["IsNew"] = true;
        ViewData["ProductId"] = 0;
        ViewData["Product"] = new Product { Active = true };
        ViewData["Categories"] = GetFlatCategories();
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Products/Edit.cshtml");
    }

    [HttpGet("edit")]
    public IActionResult Edit(int id)
    {
        using var conn = Db.GetConnection();
        var product = conn.QueryFirstOrDefault<Product>("SELECT * FROM products WHERE id=@id", new { id });
        if (product == null) return NotFound();

        ViewData["Title"] = "Edit Product";
        ViewData["Active"] = "products";
        ViewData["IsNew"] = false;
        ViewData["ProductId"] = id;
        ViewData["Product"] = product;
        ViewData["Categories"] = GetFlatCategories();
        ViewData["Errors"] = Array.Empty<string>();
        return View("~/Views/Admin/Products/Edit.cshtml");
    }

    [HttpPost("")]
    public async Task<IActionResult> Save([FromForm] IFormCollection form)
    {
        var csrfToken = form["csrf_token"].ToString();
        if (!ValidateCsrf(csrfToken)) return BadRequest("Invalid CSRF token");

        var errors = new List<string>();
        var isNew = !int.TryParse(form["id"], out var id) || id == 0;

        var name = form["name"].ToString().Trim();
        var desc = form["description"].ToString().Trim();
        var priceStr = form["price"].ToString();
        var stockStr = form["stock"].ToString();
        var catIdStr = form["category_id"].ToString();
        var active = form["active"].ToString() == "1";
        var featured = form["featured"].ToString() == "1";
        var removeImage = form["remove_image"].ToString() == "1";
        var existingImage = form["existing_image"].ToString();

        if (string.IsNullOrEmpty(name)) errors.Add("Product name is required.");
        if (!decimal.TryParse(priceStr, out var price) || price <= 0) errors.Add("Price must be a positive number.");
        if (!int.TryParse(stockStr, out var stock)) stock = 0;
        int? catId = int.TryParse(catIdStr, out var cid) ? cid : null;

        string? imageFilename = existingImage;

        // Handle image upload
        var imageFile = form.Files["image"];
        if (imageFile != null && imageFile.Length > 0)
        {
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                errors.Add("Image must be under 5MB.");
            }
            else
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowed.Contains(imageFile.ContentType))
                    errors.Add("Image must be JPEG, PNG, GIF or WebP.");
                else
                {
                    var ext = Path.GetExtension(imageFile.FileName).ToLower();
                    var filename = $"img_{Guid.NewGuid():N}{ext}";
                    var imagesPath = GetImagesPath();
                    Directory.CreateDirectory(imagesPath);
                    await using var stream = new FileStream(Path.Combine(imagesPath, filename), FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    // Delete old image
                    if (!string.IsNullOrEmpty(existingImage))
                    {
                        var old = Path.Combine(imagesPath, existingImage);
                        if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                    }
                    imageFilename = filename;
                }
            }
        }
        else if (removeImage)
        {
            if (!string.IsNullOrEmpty(existingImage))
            {
                var imagesPath = GetImagesPath();
                var old = Path.Combine(imagesPath, existingImage);
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
            }
            imageFilename = null;
        }

        if (errors.Count > 0)
        {
            var product = new Product { Id = id, Name = name, Description = desc, Price = price,
                Stock = stock, CategoryId = catId, Image = imageFilename, Active = active, Featured = featured };
            ViewData["Title"] = isNew ? "Add Product" : "Edit Product";
            ViewData["Active"] = "products";
            ViewData["IsNew"] = isNew;
            ViewData["ProductId"] = id;
            ViewData["Product"] = product;
            ViewData["Categories"] = GetFlatCategories();
            ViewData["Errors"] = errors;
            return View("~/Views/Admin/Products/Edit.cshtml");
        }

        using var conn = Db.GetConnection();
        if (isNew)
        {
            var slug = GenerateSlug(name);
            slug = EnsureUniqueSlug(conn, slug, 0);
            conn.Execute(@"INSERT INTO products (name, slug, description, price, stock, category_id, image, active, featured)
                VALUES (@name, @slug, @desc, @price, @stock, @catId, @image, @active, @featured)",
                new { name, slug, desc, price, stock, catId = catId as object ?? DBNull.Value,
                      image = imageFilename as object ?? DBNull.Value, active = active ? 1 : 0, featured = featured ? 1 : 0 });
        }
        else
        {
            conn.Execute(@"UPDATE products SET name=@name, description=@desc, price=@price, stock=@stock,
                category_id=@catId, image=@image, active=@active, featured=@featured WHERE id=@id",
                new { name, desc, price, stock, catId = catId as object ?? DBNull.Value,
                      image = imageFilename as object ?? DBNull.Value, active = active ? 1 : 0,
                      featured = featured ? 1 : 0, id });
        }

        Flash("product_saved", isNew ? "Product created." : "Product updated.");
        return Redirect("/admin/products");
    }

    [HttpGet("delete")]
    public IActionResult Delete(int id)
    {
        using var conn = Db.GetConnection();
        conn.Execute("UPDATE products SET active=0 WHERE id=@id", new { id });
        Flash("product_saved", "Product deactivated.");
        return Redirect("/admin/products");
    }

    private List<Category> GetFlatCategories()
    {
        using var conn = Db.GetConnection();
        return conn.Query<Category>(@"
            SELECT c.*, p.name as ParentName
            FROM categories c LEFT JOIN categories p ON c.parent_id=p.id
            ORDER BY COALESCE(p.name, c.name), c.parent_id IS NOT NULL, c.name").ToList();
    }

    private string GetImagesPath()
    {
        var configured = _config["ImagesPath"];
        if (!string.IsNullOrEmpty(configured))
            return Path.IsPathRooted(configured) ? configured
                : Path.Combine(Directory.GetCurrentDirectory(), configured);
        return Path.Combine(_env.WebRootPath, "images");
    }

    private static string GenerateSlug(string name)
    {
        return System.Text.RegularExpressions.Regex
            .Replace(name.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');
    }

    private static string EnsureUniqueSlug(Microsoft.Data.Sqlite.SqliteConnection conn, string slug, int excludeId)
    {
        var candidate = slug;
        var n = 1;
        while (conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM products WHERE slug=@s AND id!=@id",
            new { s = candidate, id = excludeId }) > 0)
        {
            candidate = $"{slug}-{n++}";
        }
        return candidate;
    }
}
