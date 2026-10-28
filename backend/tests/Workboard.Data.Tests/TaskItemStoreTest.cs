namespace Workboard.Data.Tests
{
    public sealed class TaskItemStoreTest : SqliteTestBase
    {
        public TaskItemStoreTest()
        {
            CreateDatabase();
        }

        [Fact]
        public void Create()
        {
            string title = "Title";
            string description = "Description";
            TaskItem item = Context.TaskItems.Create(title, description);

            Assert.Equal(title, item.Title);
            Assert.Equal(description, item.Description);
        }
    }
}
