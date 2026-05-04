using Microsoft.AspNetCore.Http;

namespace ShopDotNet.Services;

public interface ISecurityService
{
    string GetOrCreateCsrfToken(ISession session);
    bool ValidateCsrf(ISession session, string? token);
    bool IsRateLimited(string action, string ip);
    void RecordAttempt(string action, string ip);
    void ClearAttempts(string action, string ip);
}
