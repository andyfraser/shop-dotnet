using Dapper;

namespace ShopDotNet.Services;

public class SettingsService
{
    private readonly DatabaseService _db;
    private Dictionary<string, string>? _cache;

    private static readonly Dictionary<string, string> Defaults = new()
    {
        ["site_name"] = "Demo|shop",
        ["currency_symbol"] = "£",
        ["password_min_length"] = "6",
        ["login_max_attempts"] = "5",
        ["login_window_minutes"] = "15",
        ["register_max_attempts"] = "10",
        ["register_window_minutes"] = "60",
        ["low_stock_threshold"] = "10",
    };

    public SettingsService(DatabaseService db) => _db = db;

    public string Get(string key)
    {
        _cache ??= LoadAll();
        return _cache.TryGetValue(key, out var v) ? v : (Defaults.TryGetValue(key, out var d) ? d : "");
    }

    public Dictionary<string, string> GetAll()
    {
        _cache ??= LoadAll();
        // Return with defaults filled in
        var result = new Dictionary<string, string>(Defaults);
        foreach (var kv in _cache) result[kv.Key] = kv.Value;
        return result;
    }

    public void Save(Dictionary<string, string> values)
    {
        using var conn = _db.GetConnection();
        foreach (var kv in values)
        {
            conn.Execute(
                "INSERT INTO settings (key, value) VALUES (@key, @value) ON CONFLICT(key) DO UPDATE SET value=excluded.value",
                new { key = kv.Key, value = kv.Value });
        }
        _cache = null; // invalidate
    }

    private Dictionary<string, string> LoadAll()
    {
        using var conn = _db.GetConnection();
        var rows = conn.Query<(string Key, string Value)>("SELECT key, value FROM settings");
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }
}
