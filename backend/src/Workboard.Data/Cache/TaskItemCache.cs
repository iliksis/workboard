using Workboard.Domain;

namespace Workboard.Data.Cache
{
    public sealed class TaskItemCache(ApiContext context)
        : EntityCache<TaskItem>(t => t.Id, t => t.ProjectId, Comparer<TaskItem>.Create((a, b) => a.Sort.CompareTo(b.Sort)))
    {
        public TaskItem? Get(Guid id) => GetOrLoad(id, context.Stores.TaskItems.SelectById);

        /// <summary>All tasks of a project ordered by sort; group by <see cref="TaskItem.ColumnId"/> for the board view.</summary>
        public IReadOnlyList<TaskItem> GetByProject(Guid projectId) => GetChildren(projectId, context.Stores.TaskItems.SelectByProject);

        public TaskItem Create(Guid projectId, Guid? columnId, string title, string? description, float sort = 0)
        {
            EnsureColumnInProject(columnId, projectId); // before taking our lock: see lock order in EntityCache
            lock (Gate)
            {
                TaskItem item = context.Stores.TaskItems.Create(projectId, columnId, title, description, sort);
                Put(item);
                return item;
            }
        }

        public TaskItem? Update(Guid id, TaskItemUpdate update)
        {
            lock (Gate)
            {
                return Store(id, context.Stores.TaskItems.Update(id, update));
            }
        }

        /// <summary>Moves a task to another column of its project (or out of any column) with a new sort value.</summary>
        public TaskItem? Move(Guid id, Guid? columnId, float sort)
        {
            TaskItem? current = Get(id);
            if (current is null)
            {
                return null;
            }
            EnsureColumnInProject(columnId, current.ProjectId); // before taking our lock: see lock order in EntityCache
            lock (Gate)
            {
                return Store(id, context.Stores.TaskItems.Move(id, columnId, sort));
            }
        }

        public bool Delete(Guid id)
        {
            lock (Gate)
            {
                bool deleted = context.Stores.TaskItems.DeleteById(id);
                Remove(id);
                return deleted;
            }
        }

        private TaskItem? Store(Guid id, TaskItem? updated)
        {
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

        private void EnsureColumnInProject(Guid? columnId, Guid projectId)
        {
            if (columnId is null)
            {
                return;
            }
            if (context.Columns.Get(columnId.Value)?.ProjectId != projectId)
            {
                throw new InvalidOperationException($"Column {columnId} does not belong to project {projectId}.");
            }
        }
    }
}
