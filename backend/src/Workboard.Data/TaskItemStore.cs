using Microsoft.Data.Sqlite;
using Workboard.Domain;

namespace Workboard.Data
{
    internal class TaskItemStore(ApiContext context)
    {
        private const string table = "task_items";
        private const string columns = "id, project_id, column_id, title, description, sort";

        public TaskItem Create(Guid projectId, Guid? columnId, string title, string? description, float sort = 0)
        {
            TaskItem item = new(Guid.NewGuid(), projectId, columnId, title, description, sort);
            context.Execute(
                $"INSERT INTO {table} (id, project_id, column_id, title, description, sort, created_at, updated_at) VALUES (@id, @project_id, @column_id, @title, @description, @sort, @now, @now)",
                ("@id", item.Id), ("@project_id", projectId), ("@column_id", columnId), ("@title", title), ("@description", description), ("@sort", sort), ("@now", DateTimeOffset.UtcNow));
            return item;
        }

        public bool DeleteById(Guid id) => context.Execute($"DELETE FROM {table} WHERE id = @id", ("@id", id)) > 0;

        public TaskItem? SelectById(Guid id) =>
            context.Query($"SELECT {columns} FROM {table} WHERE id = @id", Map, ("@id", id)).FirstOrDefault();

        public List<TaskItem> SelectByProject(Guid projectId) =>
            context.Query($"SELECT {columns} FROM {table} WHERE project_id = @project_id", Map, ("@project_id", projectId));

        public TaskItem? Update(Guid id, TaskItemUpdate update)
        {
            context.Update(table, id, ("title", update.Title), ("description", update.Description), ("sort", update.Sort));
            return SelectById(id);
        }

        /// <summary>Places the task in a column (or none) with a new sort value.</summary>
        public TaskItem? Move(Guid id, Guid? columnId, float sort)
        {
            context.Execute($"UPDATE {table} SET column_id = @column_id, sort = @sort, updated_at = @now WHERE id = @id",
                ("@id", id), ("@column_id", columnId), ("@sort", sort), ("@now", DateTimeOffset.UtcNow));
            return SelectById(id);
        }

        private static TaskItem Map(SqliteDataReader r) =>
            new(r.GetGuid(0), r.GetGuid(1), r.IsDBNull(2) ? null : r.GetGuid(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4), r.GetFloat(5));
    }
}
