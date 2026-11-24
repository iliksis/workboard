using Workboard.Domain;

namespace Workboard.Data.Cache
{
    public sealed class ColumnCache(ApiContext context, TaskItemCache tasks)
        : EntityCache<Column>(c => c.Id, c => c.ProjectId, Comparer<Column>.Create((a, b) => a.Sort.CompareTo(b.Sort)))
    {
        public Column? Get(Guid id) => GetOrLoad(id, context.Columns.SelectById);

        public IReadOnlyList<Column> GetByProject(Guid projectId) => GetChildren(projectId, context.Columns.SelectByProject);

        public Column Create(Guid projectId, string name, float sort = 0)
        {
            lock (Gate)
            {
                Column column = context.Columns.Create(projectId, name, sort);
                Put(column);
                return column;
            }
        }

        public Column? Update(Guid id, ColumnUpdate update)
        {
            lock (Gate)
            {
                Column? updated = context.Columns.Update(id, update);
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
                Column? column = Get(id);
                bool deleted = context.Columns.DeleteById(id);
                Remove(id);
                // The database un-places the column's tasks (column_id -> NULL); drop them so they reload.
                if (column is not null)
                {
                    tasks.EvictByParent(column.ProjectId);
                }
                return deleted;
            }
        }
    }
}
