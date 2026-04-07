using System.Text.Json;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public class AuthService
{
    private const string SessionKey = "user_session";

    public static UserSession? GetCurrentUser(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (json == null) return null;
        return JsonSerializer.Deserialize<UserSession>(json);
    }

    public static void Login(ISession session, UserSession user)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(user));
    }

    public static void Logout(ISession session)
    {
        session.Remove(SessionKey);
        session.Remove("cart");
    }
}
