using Workboard.Domain;

namespace Workboard.Data.Cache
{
    public sealed class ProjectCache(ApiContext context, ColumnCache columns, TaskItemCache tasks)
        : EntityCache<Project>(p => p.Id, p => p.WorkspaceId, Comparer<Project>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)))
    {
        public Project? Get(Guid id) => GetOrLoad(id, context.Projects.SelectById);

        /// <summary>Archived projects are cached too; they are only filtered out of this list by default.</summary>
        public IReadOnlyList<Project> GetByWorkspace(Guid workspaceId, bool includeArchived = false) =>
            [.. GetChildren(workspaceId, context.Projects.SelectByWorkspace).Where(p => includeArchived || !p.IsArchived)];

        public Project Create(Guid workspaceId, string name, string? description)
        {
            lock (Gate)
            {
                Project project = context.Projects.Create(workspaceId, name, description);
                Put(project);
                return project;
            }
        }

        public Project? Update(Guid id, ProjectUpdate update)
        {
            lock (Gate)
            {
                Project? updated = context.Projects.Update(id, update);
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
                bool deleted = context.Projects.DeleteById(id);
                Remove(id);
                columns.EvictByParent(id);
                tasks.EvictByParent(id);
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
                    columns.EvictByParent(project.Id);
                    tasks.EvictByParent(project.Id);
                }
                EvictByParent(workspaceId);
            }
        }
    }
}
