using Microsoft.Data.Sqlite;
using Workboard.Domain;

namespace Workboard.Data
{
    internal class ProjectStore(ApiContext context)
    {
        private const string table = "projects";
        private const string columns = "id, workspace_id, name, description, is_archived";

        public Project Create(Guid workspaceId, string name, string? description)
        {
            Project project = new(Guid.NewGuid(), workspaceId, name, description, false);
            context.Execute(
                $"INSERT INTO {table} (id, workspace_id, name, description, created_at, updated_at) VALUES (@id, @workspace_id, @name, @description, @now, @now)",
                ("@id", project.Id), ("@workspace_id", workspaceId), ("@name", name), ("@description", description), ("@now", DateTimeOffset.UtcNow));
            return project;
        }

        public bool DeleteById(Guid id) => context.Execute($"DELETE FROM {table} WHERE id = @id", ("@id", id)) > 0;

        public Project? SelectById(Guid id) =>
            context.Query($"SELECT {columns} FROM {table} WHERE id = @id", Map, ("@id", id)).FirstOrDefault();

        public List<Project> SelectByWorkspace(Guid workspaceId) =>
            context.Query($"SELECT {columns} FROM {table} WHERE workspace_id = @workspace_id", Map, ("@workspace_id", workspaceId));

        public Project? Update(Guid id, ProjectUpdate update)
        {
            context.Update(table, id, ("name", update.Name), ("description", update.Description), ("is_archived", update.IsArchived));
            return SelectById(id);
        }

        private static Project Map(SqliteDataReader r) =>
            new(r.GetGuid(0), r.GetGuid(1), r.GetString(2), r.IsDBNull(3) ? null : r.GetString(3), r.GetBoolean(4));
    }
}
