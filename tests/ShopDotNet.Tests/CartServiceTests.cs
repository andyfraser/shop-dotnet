using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using ShopDotNet.Services;
using System.Text;
using System.Text.Json;

namespace ShopDotNet.Tests;

public class CartServiceTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly Mock<IDatabaseService> _mockDb;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _mockSession = new Mock<ISession>();
        _mockDb = new Mock<IDatabaseService>();
        _cartService = new CartService(_mockDb.Object);
    }

    private void SetupSession(Dictionary<int, int> cart)
    {
        var json = JsonSerializer.Serialize(cart);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        _mockSession.Setup(s => s.TryGetValue("cart", out bytes)).Returns(true);
    }

    [Fact]
    public void GetCount_ReturnsCorrectSum()
    {
        SetupSession(new Dictionary<int, int> { { 1, 2 }, { 2, 3 } });
        var count = _cartService.GetCount(_mockSession.Object);
        Assert.Equal(5, count);
    }

    [Fact]
    public void Add_AddsNewProduct()
    {
        byte[] capturedValue = null;
        _mockSession.Setup(s => s.Set("cart", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => capturedValue = v);

        _cartService.Add(_mockSession.Object, 1, 2);

        Assert.NotNull(capturedValue);
        var json = Encoding.UTF8.GetString(capturedValue);
        var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
        Assert.Equal(2, cart[1]);
    }

    [Fact]
    public void Add_IncrementsExistingProduct()
    {
        SetupSession(new Dictionary<int, int> { { 1, 2 } });
        byte[] capturedValue = null;
        _mockSession.Setup(s => s.Set("cart", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => capturedValue = v);

        _cartService.Add(_mockSession.Object, 1, 3);

        var json = Encoding.UTF8.GetString(capturedValue);
        var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
        Assert.Equal(5, cart[1]);
    }

    [Fact]
    public void Update_ModifiesCart()
    {
        SetupSession(new Dictionary<int, int> { { 1, 2 }, { 2, 5 } });
        byte[] capturedValue = null;
        _mockSession.Setup(s => s.Set("cart", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => capturedValue = v);

        _cartService.Update(_mockSession.Object, new Dictionary<int, int> { { 1, 0 }, { 2, 10 } });

        var json = Encoding.UTF8.GetString(capturedValue);
        var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
        Assert.False(cart.ContainsKey(1));
        Assert.Equal(10, cart[2]);
    }

    [Fact]
    public void Remove_RemovesProduct()
    {
        SetupSession(new Dictionary<int, int> { { 1, 2 }, { 2, 5 } });
        byte[] capturedValue = null;
        _mockSession.Setup(s => s.Set("cart", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => capturedValue = v);

        _cartService.Remove(_mockSession.Object, 1);

        var json = Encoding.UTF8.GetString(capturedValue);
        var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
        Assert.False(cart.ContainsKey(1));
        Assert.Equal(5, cart[2]);
    }

    [Fact]
    public void Clear_RemovesSessionKey()
    {
        _cartService.Clear(_mockSession.Object);
        _mockSession.Verify(s => s.Remove("cart"), Times.Once);
    }
}
