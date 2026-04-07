using System.Security.Cryptography;
using Dapper;

namespace ShopDotNet.Services;

public class SecurityService
{
    private readonly DatabaseService _db;
    private readonly SettingsService _settings;

    public SecurityService(DatabaseService db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public static string GetOrCreateCsrfToken(ISession session)
    {
        var token = session.GetString("csrf_token");
        if (token == null)
        {
            token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();
            session.SetString("csrf_token", token);
        }
        return token;
    }

    public static bool ValidateCsrf(ISession session, string? token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var stored = session.GetString("csrf_token");
        return stored != null && stored == token;
    }

    public bool IsRateLimited(string action, string ip)
    {
        var maxAttempts = int.Parse(_settings.Get($"{action}_max_attempts"));
        var windowMinutes = int.Parse(_settings.Get($"{action}_window_minutes"));
        var since = DateTime.UtcNow.AddMinutes(-windowMinutes).ToString("yyyy-MM-dd HH:mm:ss");

        using var conn = _db.GetConnection();
        var count = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM rate_limits WHERE action=@action AND ip_address=@ip AND created_at > @since",
            new { action, ip, since });
        return count >= maxAttempts;
    }

    public void RecordAttempt(string action, string ip)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "INSERT INTO rate_limits (action, ip_address) VALUES (@action, @ip)",
            new { action, ip });
    }

    public void ClearAttempts(string action, string ip)
    {
        using var conn = _db.GetConnection();
        conn.Execute(
            "DELETE FROM rate_limits WHERE action=@action AND ip_address=@ip",
            new { action, ip });
    }
}
