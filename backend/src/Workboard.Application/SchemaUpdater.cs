using Microsoft.Data.Sqlite;
using System.Reflection;

namespace Workboard.Data
{
    internal static class SchemaUpdater
    {
        private const string versionTable = "db_version";
        private const string scriptNameStart = "Workboard.Data.sql.";

        internal static bool Update(SqliteConnection db)
        {
            db.Open();

            Version current = GetCurrentVersion(db);
            bool success = RunMigrationScripts(db, current);

            db.Close();

            return success;
        }

        private static Version GetCurrentVersion(SqliteConnection db)
        {
            string hasVersionTableScript = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{versionTable}'";
            SqliteCommand command = new(hasVersionTableScript, db);
            SqliteDataReader reader = command.ExecuteReader();
            bool hasTable = reader.HasRows;
            reader.Close();

            if (hasTable)
            {
                string lastVersionScript = $"SELECT name FROM '{versionTable}' ORDER BY timestamp LIMIT 1";
                command = new(lastVersionScript, db);
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    string version = reader.GetString(0);
                    return new(version);
                }
            }
            else
            {
                string createTableScript = $"CREATE TABLE '{versionTable}' (name TEXT PRIMARY KEY NOT NULL, timestamp REAL NOT NULL);";
                command = new(createTableScript, db);
                command.ExecuteNonQuery();
            }

            return new(0, 0, 0);
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

        private static bool RunMigrationScripts(SqliteConnection db, Version currentVersion)
        {
            bool success = true;
            Assembly asm = typeof(ApiContext).Assembly;

            List<(string Name, Version Version)> scripts = GetMigrationScripts();

            foreach (var s in scripts.Where(s => s.Version > currentVersion))
            {
                using Stream stream = asm.GetManifestResourceStream(s.Name)!;
                using StreamReader reader = new(stream);
                string sql = reader.ReadToEnd();

                using SqliteTransaction tx = db.BeginTransaction();
                SqliteCommand command = new(sql, db, tx);
                int rows = command.ExecuteNonQuery();

                success = UpdateVersionTable(db, tx, s.Version);
                if (!success)
                {
                    tx.Rollback();
                    break;
                }

                tx.Commit();
            }

            return success;
        }

        private static bool UpdateVersionTable(SqliteConnection db, SqliteTransaction tx, Version version)
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
    }
}
