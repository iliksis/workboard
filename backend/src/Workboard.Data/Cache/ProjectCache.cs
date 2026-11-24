using Workboard.Domain;

namespace Workboard.Data.Cache
{
    public sealed class ProjectCache(ApiContext context)
        : EntityCache<Project>(p => p.Id, p => p.WorkspaceId, Comparer<Project>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)))
    {
        public Project? Get(Guid id) => GetOrLoad(id, context.Stores.Projects.SelectById);

        /// <summary>Archived projects are cached too; they are only filtered out of this list by default.</summary>
        public IReadOnlyList<Project> GetByWorkspace(Guid workspaceId, bool includeArchived = false) =>
            [.. GetChildren(workspaceId, context.Stores.Projects.SelectByWorkspace).Where(p => includeArchived || !p.IsArchived)];

        public Project Create(Guid workspaceId, string name, string? description)
        {
            lock (Gate)
            {
                Project project = context.Stores.Projects.Create(workspaceId, name, description);
                Put(project);
                return project;
            }
        }

        public Project? Update(Guid id, ProjectUpdate update)
        {
            lock (Gate)
            {
                Project? updated = context.Stores.Projects.Update(id, update);
                if (updated is null)
                {
                    Remove(id);
                }
                else
                {
                    Put(updated);
                }
                return updated;
            }
        }

        public bool Delete(Guid id)
        {
            lock (Gate)
            {
                bool deleted = context.Stores.Projects.DeleteById(id);
                Remove(id);
                context.Columns.EvictByParent(id);
                context.TaskItems.EvictByParent(id);
                return deleted;
            }
        }

        /// <summary>A workspace was deleted: the database cascaded to these projects and everything below them.</summary>
        internal void EvictWorkspace(Guid workspaceId, IEnumerable<Project> workspaceProjects)
        {
            lock (Gate)
            {
                foreach (Project project in workspaceProjects)
                {
                    context.Columns.EvictByParent(project.Id);
                    context.TaskItems.EvictByParent(project.Id);
                }
                EvictByParent(workspaceId);
            }
        }
    }
}
