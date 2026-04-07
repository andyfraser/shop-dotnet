using Microsoft.Data.Sqlite;
using Dapper;
using BCrypt.Net;

namespace ShopDotNet.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        var dbPath = config["Database:Path"] ?? "shop.db";
        _connectionString = $"Data Source={dbPath}";
        EnsureInitialized();
    }

    public SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private void EnsureInitialized()
    {
        using var conn = GetConnection();

        // Run schema
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Data", "schema.sql");
        if (!File.Exists(schemaPath))
        {
            // Try relative to current directory (dev)
            schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "schema.sql");
        }
        if (File.Exists(schemaPath))
        {
            var sql = File.ReadAllText(schemaPath);
            conn.Execute(sql);
        }

        // Seed admin user
        var adminExists = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM users WHERE email = 'admin@shop.local'");
        if (adminExists == 0)
        {
            conn.Execute(
                "INSERT INTO users (name, email, password_hash, role) VALUES (@name, @email, @hash, 'admin')",
                new { name = "Admin", email = "admin@shop.local", hash = BCrypt.Net.BCrypt.HashPassword("password") });
        }

        // Seed customer user
        var custExists = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM users WHERE email = 'jane@example.com'");
        if (custExists == 0)
        {
            conn.Execute(
                "INSERT INTO users (name, email, password_hash, role) VALUES (@name, @email, @hash, 'customer')",
                new { name = "Jane Smith", email = "jane@example.com", hash = BCrypt.Net.BCrypt.HashPassword("password") });
        }

        // Seed default settings
        var defaultSettings = new Dictionary<string, string>
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
        foreach (var kv in defaultSettings)
        {
            conn.Execute(
                "INSERT OR IGNORE INTO settings (key, value) VALUES (@key, @value)",
                new { key = kv.Key, value = kv.Value });
        }
    }
}
