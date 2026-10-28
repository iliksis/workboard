using Microsoft.Data.Sqlite;

namespace Workboard.Data.Tests;

public sealed class SchemaUpdaterTests : SqliteTestBase
{
    private readonly Version latestVersion = new(0, 0, 1);
    private const int numberOfScripts = 1;

    [Fact]
    public void Update_OnFreshDb_CreatesVersionTable_AndReturnsTrue()
    {
        Assert.True(Updater.Update());

        using SqliteConnection db = Open();
        Assert.Equal(1L, Scalar(db, "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='db_version'"));
        Assert.Equal(latestVersion, Updater.GetLatestVersion());
    }

    [Fact]
    public void Update_IsIdempotent()
    {
        Assert.True(Updater.Update());
        Assert.True(Updater.Update());

        using SqliteConnection db = Open();
        Assert.Equal(numberOfScripts, Scalar(db, "SELECT count(*) FROM db_version"));
        Assert.Equal(latestVersion, Updater.GetLatestVersion());
    }
}
