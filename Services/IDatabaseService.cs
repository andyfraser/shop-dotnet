using Microsoft.Data.Sqlite;

namespace ShopDotNet.Services;

public interface IDatabaseService
{
    SqliteConnection GetConnection();
}
