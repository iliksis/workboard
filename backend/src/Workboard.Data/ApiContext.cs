using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Workboard.Data
{
    public class ApiContext
    {
        internal WorkspaceStore Workspaces { get; }
        internal ProjectStore Projects { get; }
        internal ColumnStore Columns { get; }
        internal TaskItemStore TaskItems { get; }
        internal readonly ILoggerFactory loggerFactory;
        private readonly string connectionString;

        public ApiContext(ApiContextOptions options)
        {
            loggerFactory = options.LoggerFactory;
            // SQLite enforces foreign keys (cascades) per connection, so switch it on for every connection we hand out.
            connectionString = new SqliteConnectionStringBuilder(options.ConnectionString) { ForeignKeys = true }.ToString();

            Workspaces = new(this);
            Projects = new(this);
            Columns = new(this);
            TaskItems = new(this);
        }

        public SqliteConnection GetDbConnection() => new(connectionString);

        internal List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, params (string Name, object? Value)[] parameters)
        {
            using SqliteConnection db = GetDbConnection();
            db.Open();
            using SqliteCommand command = CreateCommand(db, sql, parameters);
            using SqliteDataReader reader = command.ExecuteReader();

            List<T> rows = [];
            while (reader.Read())
            {
                rows.Add(map(reader));
            }
            return rows;
        }

        internal int Execute(string sql, params (string Name, object? Value)[] parameters)
        {
            using SqliteConnection db = GetDbConnection();
            db.Open();
            using SqliteCommand command = CreateCommand(db, sql, parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>Sets every non-null field of the row and bumps <c>updated_at</c>. Returns the affected row count.</summary>
        internal int Update(string table, Guid id, params (string Column, object? Value)[] fields)
        {
            List<(string Name, object? Value)> parameters = [("@id", id), ("@updated_at", DateTimeOffset.UtcNow)];
            List<string> sets = ["updated_at = @updated_at"];
            foreach ((string column, object? value) in fields.Where(f => f.Value is not null))
            {
                sets.Add($"{column} = @{column}");
                parameters.Add(($"@{column}", value));
            }
            return Execute($"UPDATE {table} SET {string.Join(", ", sets)} WHERE id = @id", [.. parameters]);
        }

        internal static SqliteCommand CreateCommand(SqliteConnection db, string sql, params (string Name, object? Value)[] parameters)
        {
            SqliteCommand command = db.CreateCommand();
            command.CommandText = sql;

            foreach ((string name, object? value) in parameters)
            {
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }

            return command;
        }
    }
}
