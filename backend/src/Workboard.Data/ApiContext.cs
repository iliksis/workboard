using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Workboard.Data
{
    public class ApiContext
    {
        public TaskItemStore TaskItems { get; private set; }
        internal readonly ILoggerFactory loggerFactory;
        private readonly string connectionString;

        public ApiContext(ApiContextOptions options)
        {
            loggerFactory = options.LoggerFactory;
            connectionString = options.ConnectionString;

            TaskItems = new(this);
        }

        public SqliteConnection GetDbConnection() => new(connectionString);

        internal static SqliteCommand CreateCommand(SqliteConnection db, string sql, params (string Name, object? Value)[] parameters)
        {
            SqliteCommand command = db.CreateCommand();
            command.CommandText = sql;

            foreach ((string name, object? value) in parameters)
            {
                command.Parameters.AddWithValue(name, value);
            }

            return command;
        }
    }
}
