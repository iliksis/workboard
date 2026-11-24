using Workboard.Data.Cache;

namespace Workboard.Data.Tests.Cache
{
    public abstract class CacheTestBase : SqliteTestBase
    {
        protected CacheTestBase()
        {
            CreateDatabase();
        }

        protected TaskItemCache Tasks => Context.TaskItems;
        protected ColumnCache Columns => Context.Columns;
        protected ProjectCache Projects => Context.Projects;
        protected WorkspaceCache Workspaces => Context.Workspaces;
    }
}
