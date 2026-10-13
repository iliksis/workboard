using Microsoft.Data.Sqlite;

namespace Workboard.Data
{
    public class ApiContext(ApiContextOptions options)
    {
        public async Task InitializeDatabaseAsync()
        {
            SchemaUpdater.Update(GetDbConnection());
        }

        public SqliteConnection GetDbConnection() => new(options.ConnectionString);
    }
}
