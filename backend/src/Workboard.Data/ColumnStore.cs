using Microsoft.Data.Sqlite;
using Workboard.Domain;

namespace Workboard.Data
{
    internal class ColumnStore(ApiContext context)
    {
        private const string table = "columns";
        private const string columns = "id, project_id, name, sort";

        public Column Create(Guid projectId, string name, float sort)
        {
            Column column = new(Guid.NewGuid(), projectId, name, sort);
            context.Execute(
                $"INSERT INTO {table} (id, project_id, name, sort, created_at, updated_at) VALUES (@id, @project_id, @name, @sort, @now, @now)",
                ("@id", column.Id), ("@project_id", projectId), ("@name", name), ("@sort", sort), ("@now", DateTimeOffset.UtcNow));
            return column;
        }

        public bool DeleteById(Guid id) => context.Execute($"DELETE FROM {table} WHERE id = @id", ("@id", id)) > 0;

        public Column? SelectById(Guid id) =>
            context.Query($"SELECT {columns} FROM {table} WHERE id = @id", Map, ("@id", id)).FirstOrDefault();

        public List<Column> SelectByProject(Guid projectId) =>
            context.Query($"SELECT {columns} FROM {table} WHERE project_id = @project_id", Map, ("@project_id", projectId));

        public Column? Update(Guid id, ColumnUpdate update)
        {
            context.Update(table, id, ("name", update.Name), ("sort", update.Sort));
            return SelectById(id);
        }

        private static Column Map(SqliteDataReader r) => new(r.GetGuid(0), r.GetGuid(1), r.GetString(2), r.GetFloat(3));
    }
}
