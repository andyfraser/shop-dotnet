namespace ShopDotNet.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? ParentId { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string CreatedAt { get; set; } = "";
    public List<Category> Children { get; set; } = new();
    public string? ParentName { get; set; }
    public int ProductCount { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int? CategoryId { get; set; }
    public string? Image { get; set; }
    public bool Active { get; set; } = true;
    public bool Featured { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? CatName { get; set; }
    public string? CatSlug { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "customer";
    public string? Address { get; set; }
    public string CreatedAt { get; set; } = "";
    public int OrderCount { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Status { get; set; } = "pending";
    public decimal Total { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public string? DeliveryMethod { get; set; }
    public decimal DeliveryCost { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int ItemCount { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ProductName { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
}

public class DeliveryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;
    public decimal MinOrderTotal { get; set; }
}

public class CartItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Image { get; set; }
    public int Qty { get; set; }
    public decimal Subtotal => Price * Qty;
}

public class UserSession
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "customer";
    public string? Address { get; set; }
    public string CreatedAt { get; set; } = "";
    public bool IsAdmin => Role == "admin";
}
