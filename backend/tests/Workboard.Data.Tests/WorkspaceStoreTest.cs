using Workboard.Domain;

namespace Workboard.Data.Tests
{
    public class WorkspaceStoreTest : SqliteTestBase
    {
        public WorkspaceStoreTest()
        {
            CreateDatabase();
        }

        [Fact]
        public void Create()
        {
            string name = "Workspace Name";
            string description = "Description";
            Workspace workspace = Context.Stores.Workspaces.Create(name, description);

            Assert.Equal(name, workspace.Name);
            Assert.Equal(description, workspace.Description);
            Assert.Equal("Work", workspace.Slug);
            Assert.Equal(workspace, Context.Stores.Workspaces.SelectById(workspace.Id));
        }
    }
}
