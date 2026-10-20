using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Workboard.Data
{
    public sealed partial class SchemaUpdater(SqliteConnection db, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<SchemaUpdater> _logger = loggerFactory.CreateLogger<SchemaUpdater>();

        private const string versionTable = "db_version";
        private const string scriptNameStart = "Workboard.Data.sql.";

        public bool Update()
        {
            Version current = GetLatestVersion();
            bool success = RunMigrationScripts(current);
            return success;
        }

        public Version GetLatestVersion()
        {
            db.Open();
            Version version = new(0, 0, 0);
            string hasVersionTableScript = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{versionTable}'";
            SqliteCommand command = new(hasVersionTableScript, db);
            SqliteDataReader reader = command.ExecuteReader();
            bool hasTable = reader.HasRows;
            reader.Close();

            if (hasTable)
            {
                string lastVersionScript = $"SELECT name FROM '{versionTable}' ORDER BY rowid DESC LIMIT 1";
                command = new(lastVersionScript, db);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string _version = reader.GetString(0);
                    version = new(_version);
                }
                reader.Close();
            }
            else
            {
                string createTableScript = $"CREATE TABLE '{versionTable}' (name TEXT PRIMARY KEY NOT NULL, timestamp REAL NOT NULL);";
                command = new(createTableScript, db);
                command.ExecuteNonQuery();
            }

            db.Close();
            LogCurrentVersion(version);
            return version;
        }

        private static List<(string Name, Version Version)> GetMigrationScripts()
        {
            Assembly asm = typeof(ApiContext).Assembly;

            List<(string Name, Version Version)> scripts = [.. asm.GetManifestResourceNames()
                .Where(n => n.StartsWith(scriptNameStart, StringComparison.OrdinalIgnoreCase) && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .Select(n => (Name: n, Version: ParseFileVersion(n)))   // extract version from the name
                .OrderBy(s => s.Version)];

            return scripts;
        }

        private bool RunMigrationScripts(Version currentVersion)
        {
            bool success = true;
            Assembly asm = typeof(ApiContext).Assembly;

            List<(string Name, Version Version)> scripts = GetMigrationScripts();

            db.Open();
            foreach (var s in scripts.Where(s => s.Version > currentVersion))
            {
                LogApplyingMigration(s.Name, s.Version);
                using Stream stream = asm.GetManifestResourceStream(s.Name)!;
                using StreamReader reader = new(stream);
                string sql = reader.ReadToEnd();

                using SqliteTransaction tx = db.BeginTransaction();
                SqliteCommand command = new(sql, db, tx);
                int rows = command.ExecuteNonQuery();

                success = UpdateVersionTable(tx, s.Version);
                if (!success)
                {
                    tx.Rollback();
                    LogMigrationFailed(s.Name);
                    break;
                }

                tx.Commit();
            }
            db.Close();
            return success;
        }

        private bool UpdateVersionTable(SqliteTransaction tx, Version version)
        {
            string updateTableScript = $"INSERT INTO '{versionTable}' (name, timestamp) VALUES ('{version}', '{DateTimeOffset.UtcNow}')";
            SqliteCommand command = new(updateTableScript, db, tx);
            int rows = command.ExecuteNonQuery();
            return rows == 1;
        }

        private static Version ParseFileVersion(string fileName)
        {
            string version = fileName.Remove(0, scriptNameStart.Length).Split("-")[0];
            return Version.Parse(version);
        }

        #region logging
        [LoggerMessage(Level = LogLevel.Information, Message = "Current DB version is {Version}")]
        private partial void LogCurrentVersion(Version version);

        [LoggerMessage(Level = LogLevel.Information, Message = "Applying migration {Script} ({Version})")]
        private partial void LogApplyingMigration(string script, Version version);

        [LoggerMessage(Level = LogLevel.Error, Message = "Migration {Script} failed")]
        private partial void LogMigrationFailed(string script);
        #endregion
    }
}
