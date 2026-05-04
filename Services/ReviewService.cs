using Dapper;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class ReviewService : IReviewService
{
    private readonly IDatabaseService _db;

    public ReviewService(IDatabaseService db) => _db = db;

    public List<Review> GetProductReviews(int productId)
    {
        using var conn = _db.GetConnection();
        return conn.Query<Review>(
            "SELECT r.*, u.name as UserName FROM reviews r JOIN users u ON r.user_id = u.id WHERE r.product_id = @productId AND r.status = 'approved' ORDER BY r.created_at DESC",
            new { productId }).ToList();
    }

    public void AddReview(Review review)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "INSERT INTO reviews (product_id, user_id, rating, comment, status) VALUES (@ProductId, @UserId, @Rating, @Comment, @Status)",
            review);
    }

    public List<Review> GetAllReviews()
    {
        using var conn = _db.GetConnection();
        return conn.Query<Review>(
            "SELECT r.*, u.name as UserName, p.name as ProductName FROM reviews r JOIN users u ON r.user_id = u.id JOIN products p ON r.product_id = p.id ORDER BY r.created_at DESC")
            .ToList();
    }

    public void UpdateReviewStatus(int reviewId, string status)
    {
        using var conn = _db.GetConnection();
        conn.Execute("UPDATE reviews SET status = @status WHERE id = @reviewId", new { reviewId, status });
    }
}
