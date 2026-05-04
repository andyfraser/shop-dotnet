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
    public bool Active { get; set; }
    public bool Featured { get; set; }
    public string CreatedAt { get; set; } = "";

    // Calculated / Joined
    public string? CatName { get; set; }
    public string? CatSlug { get; set; }
    public double AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public List<AttributeValue> Attributes { get; set; } = new();
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

    public bool IsAdmin => Role == "admin";
    public int OrderCount { get; set; }
}

public class UserAddress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Label { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Postcode { get; set; } = "";
    public string Country { get; set; } = "";
    public bool IsDefault { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = "pending";
    public string CreatedAt { get; set; } = "";

    // Joined
    public string? UserName { get; set; }
    public string? ProductName { get; set; }
}

public class Wishlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public string CreatedAt { get; set; } = "";

    // Joined
    public string? ProductName { get; set; }
    public string? ProductSlug { get; set; }
    public decimal ProductPrice { get; set; }
    public string? ProductImage { get; set; }
}

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
    public string CreatedAt { get; set; } = "";

    // Joined
    public string? CreatedByName { get; set; }
}

public class Attribute
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AttributeValue
{
    public int Id { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = "";

    // Joined
    public string? AttributeName { get; set; }
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
    public int OrderCount { get; set; }
}
