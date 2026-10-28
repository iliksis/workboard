using Microsoft.Data.Sqlite;
using System.Text;

namespace Workboard.Data
{
    public class TaskItemStore(ApiContext context)
    {
        private const string tableName = "task_items";

        public TaskItem Create(string title, string description)
        {
            TaskItem item = new(Guid.NewGuid(), title, description);

            using SqliteConnection db = context.GetDbConnection();
            db.Open();

            SqliteCommand command = ApiContext.CreateCommand(db,
                $"INSERT INTO {tableName} (id, title, description, created_at, updated_at) VALUES (@id, @title, @description, @created_at, @updated_at)",
                ("@id", item.Id),
                ("@title", item.Title),
                ("@description", item.Description),
                ("@created_at", DateTimeOffset.UtcNow),
                ("@updated_at", DateTimeOffset.UtcNow));
            command.ExecuteNonQuery();

            db.Close();
            return item;
        }

        public bool DeleteById(Guid id)
        {
            using SqliteConnection db = context.GetDbConnection();
            db.Open();

            SqliteCommand command = ApiContext.CreateCommand(db, $"DELETE FROM {tableName} WHERE id = @id", ("@id", id));
            command.ExecuteNonQuery();

            db.Close();
            return true;
        }

        public TaskItem? SelectById(Guid id)
        {
            TaskItem? item = null;

            using SqliteConnection db = context.GetDbConnection();
            db.Open();

            SqliteCommand command = ApiContext.CreateCommand(db, $"SELECT * FROM {tableName} WHERE id = @id LIMIT 1", ("@id", id.ToString()));
            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                Guid projectId = new(reader.GetString(1));
                string title = reader.GetString(2);
                string? description = reader.IsDBNull(3) ? null : reader.GetString(3);
                float sort = reader.GetFloat(4);

                item = new(id, title, description);
            }

            db.Close();
            return item;
        }

        public IEnumerable<TaskItem> SelectByIds(IEnumerable<Guid> ids)
        {
            IList<TaskItem> items = [];

            using SqliteConnection db = context.GetDbConnection();
            db.Open();

            bool first = true;
            List<(string Name, object? Value)> parameters = [];
            StringBuilder builder = new($"SELECT * FROM {tableName} WHERE id IN (");
            for (int i = 0; i < ids.Count(); i++)
            {
                if (!first) builder.Append(", ");
                string handler = $"@id{i}";
                builder.Append(handler);
                parameters.Add(new(handler, ids.ElementAt(i)));
                first = false;
            }
            SqliteCommand command = ApiContext.CreateCommand(db, builder.ToString(), [.. parameters]);
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Guid id = new(reader.GetString(0));
                Guid projectId = new(reader.GetString(1));
                string title = reader.GetString(2);
                string? description = reader.IsDBNull(3) ? null : reader.GetString(3);
                float sort = reader.GetFloat(4);

                TaskItem item = new(id, title, description);
                items.Add(item);
            }


            return items;
        }

        public TaskItem Update(Guid id, TaskItem item)
        {
            throw new NotImplementedException();
        }
    }
}
