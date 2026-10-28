using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Workboard.Data.Tests;

/// <summary>
/// Base class for tests that need a SQLite database. Each test class instance gets its own
/// temp database file; <see cref="Dispose"/> deletes it after the test completes.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private SchemaUpdater? _updater;
    private ApiContext? _context;

    protected SqliteTestBase()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"workboard-{Guid.NewGuid():N}.db");
    }

    /// <summary>
    /// Path of the test database file. The file is created lazily when first opened.
    /// </summary>
    protected string DbPath { get; }

    protected string ConnectionString =>
        new SqliteConnectionStringBuilder { DataSource = DbPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();

    /// <summary>
    /// Lazily created <see cref="SchemaUpdater"/> bound to the test database.
    /// </summary>
    protected SchemaUpdater Updater => _updater ??= new(new(ConnectionString), NullLoggerFactory.Instance);

    /// <summary>
    /// Lazily created <see cref="ApiContext"/> bound to the test database.
    /// </summary>
    protected ApiContext Context => _context ??= new(new()
    {
        ConnectionString = ConnectionString,
        LoggerFactory = NullLoggerFactory.Instance,
    });

    /// <summary>
    /// Creates the database by running all pending migration scripts.
    /// </summary>
    protected void CreateDatabase() => Assert.True(Updater.Update());

    /// <summary>
    /// Deletes the database file, releasing any pooled connections first.
    /// </summary>
    protected void DeleteDatabase()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(DbPath);
    }

    protected SqliteConnection Open()
    {
        SqliteConnection connection = new(ConnectionString);
        connection.Open();
        return connection;
    }

    protected static long Scalar(SqliteConnection db, string sql) =>
        (long)new SqliteCommand(sql, db).ExecuteScalar()!;

    public void Dispose()
    {
        DeleteDatabase();
        GC.SuppressFinalize(this);
    }
}
