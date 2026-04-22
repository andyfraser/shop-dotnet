using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using ShopDotNet.Services;
using System.Text;

namespace ShopDotNet.Tests;

public class SecurityServiceTests
{
    [Fact]
    public void GetOrCreateCsrfToken_CreatesNew_IfNoneExists()
    {
        var mockSession = new Mock<ISession>();
        byte[] value = null;
        mockSession.Setup(s => s.TryGetValue("csrf_token", out value)).Returns(false);
        mockSession.Setup(s => s.Set("csrf_token", It.IsAny<byte[]>()))
            .Callback<string, byte[]>((k, v) => value = v);

        var token = SecurityService.GetOrCreateCsrfToken(mockSession.Object);

        Assert.NotNull(token);
        Assert.Equal(64, token.Length); // 32 bytes hex encoded
        mockSession.Verify(s => s.Set("csrf_token", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void GetOrCreateCsrfToken_ReturnsExisting_IfPresent()
    {
        var mockSession = new Mock<ISession>();
        var existingToken = "existing_token";
        byte[] existingBytes = Encoding.UTF8.GetBytes(existingToken);
        mockSession.Setup(s => s.TryGetValue("csrf_token", out existingBytes)).Returns(true);

        var token = SecurityService.GetOrCreateCsrfToken(mockSession.Object);

        Assert.Equal(existingToken, token);
        mockSession.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public void ValidateCsrf_ReturnsTrue_WhenMatching()
    {
        var mockSession = new Mock<ISession>();
        var token = "token123";
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        mockSession.Setup(s => s.TryGetValue("csrf_token", out tokenBytes)).Returns(true);

        var result = SecurityService.ValidateCsrf(mockSession.Object, token);

        Assert.True(result);
    }

    [Fact]
    public void ValidateCsrf_ReturnsFalse_WhenNotMatching()
    {
        var mockSession = new Mock<ISession>();
        var token = "token123";
        byte[] storedBytes = Encoding.UTF8.GetBytes("different");
        mockSession.Setup(s => s.TryGetValue("csrf_token", out storedBytes)).Returns(true);

        var result = SecurityService.ValidateCsrf(mockSession.Object, token);

        Assert.False(result);
    }

    [Fact]
    public void ValidateCsrf_ReturnsFalse_WhenNull()
    {
        var mockSession = new Mock<ISession>();
        var result = SecurityService.ValidateCsrf(mockSession.Object, null);
        Assert.False(result);
    }
}
