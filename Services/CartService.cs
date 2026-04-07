using System.Text.Json;
using Dapper;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class CartService
{
    private readonly DatabaseService _db;

    public CartService(DatabaseService db) => _db = db;

    private static Dictionary<int, int> GetCart(ISession session)
    {
        var json = session.GetString("cart");
        if (json == null) return new();
        return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new();
    }

    private static void SaveCart(ISession session, Dictionary<int, int> cart)
    {
        session.SetString("cart", JsonSerializer.Serialize(cart));
    }

    public int GetCount(ISession session)
    {
        return GetCart(session).Values.Sum();
    }

    public void Add(ISession session, int productId, int qty)
    {
        var cart = GetCart(session);
        cart.TryGetValue(productId, out var existing);
        cart[productId] = existing + qty;
        SaveCart(session, cart);
    }

    public void Update(ISession session, Dictionary<int, int> updates)
    {
        var cart = GetCart(session);
        foreach (var kv in updates)
        {
            if (kv.Value <= 0)
                cart.Remove(kv.Key);
            else
                cart[kv.Key] = kv.Value;
        }
        SaveCart(session, cart);
    }

    public void Remove(ISession session, int productId)
    {
        var cart = GetCart(session);
        cart.Remove(productId);
        SaveCart(session, cart);
    }

    public void Clear(ISession session)
    {
        session.Remove("cart");
    }

    public List<CartItem> GetItems(ISession session)
    {
        var cart = GetCart(session);
        if (cart.Count == 0) return new();

        var ids = string.Join(",", cart.Keys);
        using var conn = _db.GetConnection();
        var products = conn.Query<Product>(
            $"SELECT id, name, slug, price, stock, image FROM products WHERE id IN ({ids})").ToList();

        return products.Select(p => new CartItem
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Price = p.Price,
            Stock = p.Stock,
            Image = p.Image,
            Qty = cart.TryGetValue(p.Id, out var q) ? q : 0,
        }).Where(i => i.Qty > 0).ToList();
    }

    public decimal GetTotal(ISession session)
    {
        return GetItems(session).Sum(i => i.Subtotal);
    }

    public Dictionary<int, int> GetRaw(ISession session) => GetCart(session);
}
