using Workboard.Data.Cache;

namespace Workboard.Data.Tests.Cache
{
    public abstract class CacheTestBase : SqliteTestBase
    {
        protected TaskItemCache Tasks { get; }
        protected ColumnCache Columns { get; }
        protected ProjectCache Projects { get; }
        protected WorkspaceCache Workspaces { get; }

        protected CacheTestBase()
        {
            CreateDatabase();
            Tasks = new(Context);
            Columns = new(Context, Tasks);
            Projects = new(Context, Columns, Tasks);
            Workspaces = new(Context, Projects);
        }
    }
}
