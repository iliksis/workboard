using Workboard.Domain;

namespace Workboard.Data.Cache
{
    public sealed class WorkspaceCache(ApiContext context)
        : EntityCache<Workspace>(w => w.Id, _ => Guid.Empty)
    {
        private readonly Dictionary<string, Guid> idsBySlug = new(StringComparer.OrdinalIgnoreCase);

        public Workspace? Get(Guid id) => Track(GetOrLoad(id, context.Stores.Workspaces.SelectById));

        public Workspace? GetBySlug(string slug)
        {
            lock (Gate)
            {
                if (idsBySlug.TryGetValue(slug, out Guid id))
                {
                    return Get(id);
                }
                Workspace? workspace = context.Stores.Workspaces.SelectBySlug(slug);
                if (workspace is not null)
                {
                    Put(workspace);
                    idsBySlug[workspace.Slug] = workspace.Id;
                }
                return workspace;
            }
        }

        public Workspace Create(string name, string? description)
        {
            lock (Gate)
            {
                Workspace workspace = context.Stores.Workspaces.Create(name, description);
                Put(workspace);
                idsBySlug[workspace.Slug] = workspace.Id;
                return workspace;
            }
        }

        public Workspace? Update(Guid id, WorkspaceUpdate update)
        {
            lock (Gate)
            {
                Forget(id);
                Workspace? updated = context.Stores.Workspaces.Update(id, update);
                if (updated is not null)
                {
                    Put(updated);
                    idsBySlug[updated.Slug] = updated.Id;
                }
                return updated;
            }
        }

        public bool Delete(Guid id)
        {
            lock (Gate)
            {
                // Load the projects first: the database cascade removes them, and their children, with the workspace.
                IReadOnlyList<Project> workspaceProjects = context.Projects.GetByWorkspace(id, includeArchived: true);
                bool deleted = context.Stores.Workspaces.DeleteById(id);
                Forget(id);
                Remove(id);
                context.Projects.EvictWorkspace(id, workspaceProjects);
                return deleted;
            }
        }

        public override void Clear()
        {
            lock (Gate)
            {
                idsBySlug.Clear();
                base.Clear();
            }
        }

        private Workspace? Track(Workspace? workspace)
        {
            if (workspace is not null)
            {
                lock (Gate)
                {
                    idsBySlug[workspace.Slug] = workspace.Id;
                }
            }
            return workspace;
        }

        private void Forget(Guid id)
        {
            foreach (string slug in idsBySlug.Where(kv => kv.Value == id).Select(kv => kv.Key).ToList())
            {
                idsBySlug.Remove(slug);
            }
        }
    }
}
