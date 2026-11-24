using Microsoft.Data.Sqlite;
using Workboard.Domain;

namespace Workboard.Data
{
    internal class WorkspaceStore(ApiContext context)
    {
        private const string table = "workspaces";
        private const string columns = "id, name, slug, description";

        public Workspace Create(string name, string? description)
        {
            Workspace workspace = new(Guid.NewGuid(), name, CreateSlugFromTitle(name), description);
            context.Execute(
                $"INSERT INTO {table} (id, name, slug, description, created_at, updated_at) VALUES (@id, @name, @slug, @description, @now, @now)",
                ("@id", workspace.Id), ("@name", workspace.Name), ("@slug", workspace.Slug), ("@description", workspace.Description), ("@now", DateTimeOffset.UtcNow));
            return workspace;
        }

        private string CreateSlugFromTitle(string title)
        {
            string allSlug = title.Replace(' ', '-');
            foreach (int length in new[] { 4, 8, 12, 16 })
            {
                string candidate = allSlug[..Math.Min(length, allSlug.Length)];
                if (SelectBySlug(candidate) is null)
                {
                    return candidate;
                }
            }
            return allSlug[..Math.Min(8, allSlug.Length)] + "-" + Guid.NewGuid();
        }

        public bool DeleteById(Guid id) => context.Execute($"DELETE FROM {table} WHERE id = @id", ("@id", id)) > 0;

        public Workspace? SelectById(Guid id) =>
            context.Query($"SELECT {columns} FROM {table} WHERE id = @id", Map, ("@id", id)).FirstOrDefault();

        public Workspace? SelectBySlug(string slug) =>
            context.Query($"SELECT {columns} FROM {table} WHERE slug = @slug", Map, ("@slug", slug)).FirstOrDefault();

        public Workspace? Update(Guid id, WorkspaceUpdate update)
        {
            context.Update(table, id, ("name", update.Name), ("slug", update.Slug), ("description", update.Description));
            return SelectById(id);
        }

        private static Workspace Map(SqliteDataReader r) =>
            new(r.GetGuid(0), r.GetString(1), r.GetString(2), r.IsDBNull(3) ? null : r.GetString(3));
    }
}
