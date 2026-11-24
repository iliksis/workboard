using Workboard.Domain;

namespace Workboard.Data.Tests.Cache
{
    public sealed class CacheTests : CacheTestBase
    {
        private (Workspace Workspace, Project Project) Seed()
        {
            Workspace workspace = Workspaces.Create("Workspace", null);
            Project project = Projects.Create(workspace.Id, "Project", null);
            return (workspace, project);
        }

        [Fact]
        public void Get_IsServedFromMemory_AfterFirstRead()
        {
            Workspace workspace = Context.Workspaces.Create("Behind", null); // created behind the cache's back

            Workspace? first = Workspaces.Get(workspace.Id);
            Context.Workspaces.Update(workspace.Id, new WorkspaceUpdate(Name: "Changed"));
            Workspace? second = Workspaces.Get(workspace.Id);

            Assert.Equal("Behind", first?.Name);
            Assert.Equal("Behind", second?.Name); // still the cached instance
            Assert.Same(first, second);
        }

        [Fact]
        public void Get_Unknown_ReturnsNull_AndIsNotCached()
        {
            Guid id = Guid.NewGuid();
            Assert.Null(Workspaces.Get(id));
            Assert.Null(Workspaces.Get(id));
        }

        [Fact]
        public void Update_WritesThrough()
        {
            Workspace workspace = Workspaces.Create("Workspace", null);

            Workspace? updated = Workspaces.Update(workspace.Id, new WorkspaceUpdate(Description: "New"));

            Assert.Equal("New", updated?.Description);
            Assert.Same(updated, Workspaces.Get(workspace.Id));
            Assert.Equal("New", Context.Workspaces.SelectById(workspace.Id)?.Description);
        }

        [Fact]
        public void GetBySlug_FollowsSlugRename()
        {
            Workspace workspace = Workspaces.Create("Workspace", null);
            Assert.Equal(workspace.Id, Workspaces.GetBySlug(workspace.Slug.ToUpperInvariant())?.Id);

            Workspaces.Update(workspace.Id, new WorkspaceUpdate(Slug: "renamed"));

            Assert.Null(Workspaces.GetBySlug(workspace.Slug));
            Assert.Equal(workspace.Id, Workspaces.GetBySlug("renamed")?.Id);
        }

        [Fact]
        public void GetByWorkspace_LoadsOnce_AndHidesArchivedByDefault()
        {
            (Workspace workspace, Project project) = Seed();
            Project archived = Projects.Create(workspace.Id, "Old", null);
            Projects.Update(archived.Id, new ProjectUpdate(IsArchived: true));
            Projects.Clear();

            IReadOnlyList<Project> active = Projects.GetByWorkspace(workspace.Id);
            Context.Projects.Create(workspace.Id, "Behind", null); // not visible: parent already loaded
            IReadOnlyList<Project> all = Projects.GetByWorkspace(workspace.Id, includeArchived: true);

            Assert.Equal([project.Id], active.Select(p => p.Id));
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public void Columns_AreOrderedBySort()
        {
            (_, Project project) = Seed();
            Column b = Columns.Create(project.Id, "B", 2);
            Column a = Columns.Create(project.Id, "A", 1);

            Assert.Equal([a.Id, b.Id], Columns.GetByProject(project.Id).Select(c => c.Id));
        }

        [Fact]
        public void Move_ChangesColumnAndSort_AndRejectsColumnOfAnotherProject()
        {
            (Workspace workspace, Project project) = Seed();
            Project other = Projects.Create(workspace.Id, "Other", null);
            Column column = Columns.Create(project.Id, "Todo");
            Column foreign = Columns.Create(other.Id, "Todo");
            TaskItem task = Tasks.Create(project.Id, null, "Task", null);

            TaskItem? moved = Tasks.Move(task.Id, column.Id, 5);

            Assert.Equal(column.Id, moved?.ColumnId);
            Assert.Equal(5, moved?.Sort);
            Assert.Same(moved, Tasks.Get(task.Id));
            Assert.Throws<InvalidOperationException>(() => Tasks.Move(task.Id, foreign.Id, 1));
            Assert.Throws<InvalidOperationException>(() => Tasks.Create(project.Id, foreign.Id, "Bad", null));
        }

        [Fact]
        public void DeleteColumn_UnplacesItsTasks()
        {
            (_, Project project) = Seed();
            Column column = Columns.Create(project.Id, "Todo");
            TaskItem task = Tasks.Create(project.Id, column.Id, "Task", null);
            Assert.Single(Tasks.GetByProject(project.Id));

            Assert.True(Columns.Delete(column.Id));

            Assert.Null(Columns.Get(column.Id));
            Assert.Null(Tasks.Get(task.Id)?.ColumnId);
        }

        [Fact]
        public void DeleteProject_EvictsColumnsAndTasks()
        {
            (_, Project project) = Seed();
            Column column = Columns.Create(project.Id, "Todo");
            TaskItem task = Tasks.Create(project.Id, column.Id, "Task", null);

            Assert.True(Projects.Delete(project.Id));

            Assert.Null(Projects.Get(project.Id));
            Assert.Null(Columns.Get(column.Id));
            Assert.Null(Tasks.Get(task.Id));
            Assert.Empty(Columns.GetByProject(project.Id));
            Assert.Empty(Tasks.GetByProject(project.Id));
        }

        [Fact]
        public void DeleteWorkspace_EvictsProjectsColumnsAndTasks()
        {
            (Workspace workspace, Project project) = Seed();
            Column column = Columns.Create(project.Id, "Todo");
            TaskItem task = Tasks.Create(project.Id, column.Id, "Task", null);

            Assert.True(Workspaces.Delete(workspace.Id));

            Assert.Null(Workspaces.Get(workspace.Id));
            Assert.Null(Workspaces.GetBySlug(workspace.Slug));
            Assert.Null(Projects.Get(project.Id));
            Assert.Null(Columns.Get(column.Id));
            Assert.Null(Tasks.Get(task.Id));
        }
    }
}
