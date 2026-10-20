using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Workboard.Data
{
    public class ApiContext(ApiContextOptions options)
    {
        internal readonly ILoggerFactory loggerFactory = options.LoggerFactory;
        private readonly string connectionString = options.ConnectionString;

        public SqliteConnection GetDbConnection() => new(connectionString);
    }
}
