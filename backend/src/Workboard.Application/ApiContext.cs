using Microsoft.Data.Sqlite;

namespace Workboard.Data
{
    public class ApiContext (string dbName)
    {
        private string dbPath = string.Empty;

        public async Task<string> InitializeDatabaseAsync()
        {
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), dbName);
            if (!File.Exists(dbPath))
            {
                File.Create(dbPath);
            }

            using var db = GetDbConnection();
            db.Open();

            string tableCommand = "CREATE TABLE IF NOT " +
                "EXISTS MyTable (Primary_Key INTEGER PRIMARY KEY, " +
                "Text_Entry NVARCHAR(2048) NULL)";

            var createTable = new SqliteCommand(tableCommand, db);

            createTable.ExecuteReader();
            db.Close();

            return dbPath;
        }

        public SqliteConnection GetDbConnection() => new($"Filename={dbPath}");
    }
}
