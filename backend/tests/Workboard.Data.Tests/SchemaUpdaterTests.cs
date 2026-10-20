using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Workboard.Data.Tests;

public sealed class SchemaUpdaterTests : IDisposable
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"workboard-{Guid.NewGuid():N}.db");
    private string ConnectionString =>
        new SqliteConnectionStringBuilder { DataSource = _dbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
    private SchemaUpdater? _updater;
    private SchemaUpdater GetUpdater()
    {
        _updater ??= new(new(ConnectionString), NullLoggerFactory.Instance);
        return _updater;
    }

    private readonly Version latestVersion = new(0, 0, 1);
    private const int numberOfScripts = 1;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Fact]
    public void Update_OnFreshDb_CreatesVersionTable_AndReturnsTrue()
    {
        Assert.True(GetUpdater().Update());

        using SqliteConnection db = Open();
        Assert.Equal(1L, Scalar(db, "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='db_version'"));
        Assert.Equal(latestVersion, GetUpdater().GetLatestVersion());
    }

    [Fact]
    public void Update_IsIdempotent()
    {
        Assert.True(GetUpdater().Update());
        Assert.True(GetUpdater().Update());

        using SqliteConnection db = Open();
        Assert.Equal(numberOfScripts, Scalar(db, "SELECT count(*) FROM db_version"));
        Assert.Equal(latestVersion, GetUpdater().GetLatestVersion());
    }


    private SqliteConnection Open() { var c = new SqliteConnection(ConnectionString); c.Open(); return c; }

    private static long Scalar(SqliteConnection db, string sql) => (long)new SqliteCommand(sql, db).ExecuteScalar()!;
}
