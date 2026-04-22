using Xunit;
using ShopDotNet.Helpers;

namespace ShopDotNet.Tests;

public class ViewHelpersTests
{
    [Theory]
    [InlineData("£", 10.5, "£10.50")]
    [InlineData("$", 10.5, "$10.50")]
    [InlineData(null, 10.5, "£10.50")]
    public void Money_FormatsCorrectly(string symbol, decimal value, string expected)
    {
        var result = V.Money(symbol, value);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsNew_ReturnsTrue_ForRecentDate()
    {
        var recentDate = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss");
        Assert.True(V.IsNew(recentDate));
    }

    [Fact]
    public void IsNew_ReturnsFalse_ForOldDate()
    {
        var oldDate = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss");
        Assert.False(V.IsNew(oldDate));
    }

    [Fact]
    public void IsNew_ReturnsFalse_ForNull()
    {
        Assert.False(V.IsNew(null));
    }

    [Fact]
    public void PadOrder_PadsCorrectly()
    {
        Assert.Equal("000123", V.PadOrder(123));
    }

    [Theory]
    [InlineData("pending", "badge-warning")]
    [InlineData("delivered", "badge-success")]
    [InlineData("unknown", "badge-neutral")]
    [InlineData(null, "badge-neutral")]
    public void StatusBadge_ReturnsCorrectClass(string status, string expected)
    {
        Assert.Equal(expected, V.StatusBadge(status));
    }

    [Fact]
    public void ImgSrc_ReturnsPlaceholder_WhenEmpty()
    {
        Assert.Equal("/images/placeholder.svg", V.ImgSrc(null));
        Assert.Equal("/images/placeholder.svg", V.ImgSrc(""));
    }

    [Fact]
    public void ImgSrc_ReturnsPath_WhenNotEmpty()
    {
        Assert.Equal("/images/test.jpg", V.ImgSrc("test.jpg"));
    }

    [Fact]
    public void Titlecase_CapitalizesCorrectly()
    {
        Assert.Equal("Hello World", V.Titlecase("hello world"));
    }
}
