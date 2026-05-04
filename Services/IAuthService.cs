using Microsoft.AspNetCore.Http;
using ShopDotNet.Models;

namespace ShopDotNet.Services;

public interface IAuthService
{
    UserSession? GetCurrentUser(ISession session);
    void Login(ISession session, UserSession user);
    void Logout(ISession session);
}
