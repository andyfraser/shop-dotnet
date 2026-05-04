using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface IWishlistService
{
    List<Wishlist> GetUserWishlist(int userId);
    void AddToWishlist(int userId, int productId);
    void RemoveFromWishlist(int userId, int productId);
    bool IsInWishlist(int userId, int productId);
}
