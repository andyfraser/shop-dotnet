using Dapper;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class WishlistService : IWishlistService
{
    private readonly IDatabaseService _db;

    public WishlistService(IDatabaseService db) => _db = db;

    public List<Wishlist> GetUserWishlist(int userId)
    {
        using var conn = _db.GetConnection();
        return conn.Query<Wishlist>(
            @"SELECT w.*, p.name as ProductName, p.slug as ProductSlug, p.price as ProductPrice, p.image as ProductImage 
              FROM wishlists w JOIN products p ON w.product_id = p.id 
              WHERE w.user_id = @userId ORDER BY w.created_at DESC",
            new { userId }).ToList();
    }

    public void AddToWishlist(int userId, int productId)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "INSERT OR IGNORE INTO wishlists (user_id, product_id) VALUES (@userId, @productId)",
            new { userId, productId });
    }

    public void RemoveFromWishlist(int userId, int productId)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "DELETE FROM wishlists WHERE user_id = @userId AND product_id = @productId",
            new { userId, productId });
    }

    public bool IsInWishlist(int userId, int productId)
    {
        using var conn = _db.GetConnection();
        return conn.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM wishlists WHERE user_id = @userId AND product_id = @productId",
            new { userId, productId }) > 0;
    }
}
