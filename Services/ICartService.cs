using Microsoft.AspNetCore.Http;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface ICartService
{
    int GetCount(ISession session);
    void Add(ISession session, int productId, int qty);
    void Update(ISession session, Dictionary<int, int> updates);
    void Remove(ISession session, int productId);
    void Clear(ISession session);
    List<CartItem> GetItems(ISession session);
    decimal GetTotal(ISession session);
    Dictionary<int, int> GetRaw(ISession session);
}
