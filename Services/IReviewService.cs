using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface IReviewService
{
    List<Review> GetProductReviews(int productId);
    void AddReview(Review review);
    List<Review> GetAllReviews();
    void UpdateReviewStatus(int reviewId, string status);
}
