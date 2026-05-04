using System.Text.Json;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class AuthService : IAuthService
{
    private const string SessionKey = "user_session";

    public UserSession? GetCurrentUser(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (json == null) return null;
        return JsonSerializer.Deserialize<UserSession>(json);
    }

    public void Login(ISession session, UserSession user)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(user));
    }

    public void Logout(ISession session)
    {
        session.Remove(SessionKey);
        session.Remove("cart");
    }
}
