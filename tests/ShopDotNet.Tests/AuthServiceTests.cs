using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using ShopDotNet.Services;
using ShopDotNet.Models;
using System.Text;
using System.Text.Json;

namespace ShopDotNet.Tests;

public class AuthServiceTests
{
    private const string SessionKey = "user_session";
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService();
    }

    [Fact]
    public void GetCurrentUser_ReturnsNull_WhenNoSession()
    {
        var mockSession = new Mock<ISession>();
        byte[] value = null;
        mockSession.Setup(s => s.TryGetValue(SessionKey, out value)).Returns(false);

        var user = _authService.GetCurrentUser(mockSession.Object);

        Assert.Null(user);
    }

    [Fact]
    public void GetCurrentUser_ReturnsUser_WhenSessionExists()
    {
        var mockSession = new Mock<ISession>();
        var user = new UserSession { Id = 1, Name = "Test User", Email = "test@example.com", Role = "customer" };
        var json = JsonSerializer.Serialize(user);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        mockSession.Setup(s => s.TryGetValue(SessionKey, out bytes)).Returns(true);

        var result = _authService.GetCurrentUser(mockSession.Object);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
    }

    [Fact]
    public void Login_SetsSession()
    {
        var mockSession = new Mock<ISession>();
        var user = new UserSession { Id = 1, Name = "Test User" };
        byte[] capturedValue = null;
        mockSession.Setup(s => s.Set(SessionKey, It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => capturedValue = v);

        _authService.Login(mockSession.Object, user);

        Assert.NotNull(capturedValue);
        var json = Encoding.UTF8.GetString(capturedValue);
        var result = JsonSerializer.Deserialize<UserSession>(json);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public void Logout_ClearsSession()
    {
        var mockSession = new Mock<ISession>();
        
        _authService.Logout(mockSession.Object);

        mockSession.Verify(s => s.Remove(SessionKey), Times.Once);
        mockSession.Verify(s => s.Remove("cart"), Times.Once);
    }
}
