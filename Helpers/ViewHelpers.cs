namespace ShopDotNet.Helpers;

public static class V
{
    public static string Money(string? symbol, decimal value)
    {
        symbol ??= "£";
        return $"{symbol}{value:F2}";
    }

    public static bool IsNew(string? createdAt)
    {
        if (createdAt == null) return false;
        if (DateTime.TryParse(createdAt, out var dt))
            return (DateTime.UtcNow - dt).TotalDays < 7;
        return false;
    }

    public static string PadOrder(int id) => id.ToString("D6");

    public static string StatusBadge(string? status) => status switch
    {
        "pending"   => "badge-warning",
        "confirmed" => "badge-info",
        "shipped"   => "badge-info",
        "delivered" => "badge-success",
        "cancelled" => "badge-danger",
        _ => "badge-neutral"
    };

    public static string ImgSrc(string? filename) =>
        string.IsNullOrEmpty(filename) ? "/images/placeholder.svg" : $"/images/{filename}";

    public static string Titlecase(string? s) =>
        string.IsNullOrEmpty(s) ? "" :
        System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
}
