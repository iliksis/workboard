using Microsoft.Data.Sqlite;
using Workboard.Domain;

namespace Workboard.Data
{
    public class BoardCache(ApiContext context)
    {
        public async Task<Board> GetBoardAsync(Guid boardId)
        {
            using SqliteConnection db = context.GetDbConnection();
            db.Open();
            string tableCommand = "CREATE TABLE IF NOT " +
                "EXISTS MyTable (Primary_Key INTEGER PRIMARY KEY, " +
                "Text_Entry NVARCHAR(2048) NULL)";

            var createTable = new SqliteCommand(tableCommand, db);

            createTable.ExecuteReader();
            db.Close();

            return new()
            {
                Id = Guid.NewGuid(),
                Title = "Test",
            };
        }
    }
}
