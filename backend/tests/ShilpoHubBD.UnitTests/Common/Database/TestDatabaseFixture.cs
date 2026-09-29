using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Interceptors;

namespace ShilpoHubBD.UnitTests.Common.Database;

/// <summary>
/// Creates a throwaway Postgres database for the test run, builds its schema from the real EF migrations,
/// and drops it again when the run ends. The server comes from the <see cref="EnvironmentVariable"/>
/// connection string; its own database is only used to issue CREATE/DROP DATABASE and is never written to.
/// Without the variable, database tests are skipped.
/// </summary>
public sealed class TestDatabaseFixture : IAsyncLifetime
{
    public const string EnvironmentVariable = "SHILPOHUB_TEST_DB";
    private const string DatabasePrefix = "shilpohub_unit_";

    private string? _serverConnectionString;
    private string? _databaseName;

    public string? ConnectionString { get; private set; }

    public string SkipReason { get; private set; } =
        $"Set {EnvironmentVariable} to a Postgres server connection string to run database tests.";

    public async ValueTask InitializeAsync()
    {
        var server = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (string.IsNullOrWhiteSpace(server))
        {
            return;
        }

        _serverConnectionString = server;
        _databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");

        await using (var connection = new NpgsqlConnection(server))
        {
            await connection.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
            await create.ExecuteNonQueryAsync();
        }

        ConnectionString = new NpgsqlConnectionStringBuilder(server) { Database = _databaseName }.ConnectionString;

        await using var context = CreateContext(new DbContextOptionsBuilder<ShilpoHubDbContext>().UseNpgsql(ConnectionString));
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_serverConnectionString is null || _databaseName is null || !_databaseName.StartsWith(DatabasePrefix, StringComparison.Ordinal))
        {
            return;
        }

        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_serverConnectionString);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await drop.ExecuteNonQueryAsync();
    }

    /// <summary>Starts an isolated scope: everything written through it is rolled back when it is disposed.</summary>
    public async Task<DatabaseScope> BeginAsync()
    {
        Assert.SkipWhen(ConnectionString is null, SkipReason);
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        return new DatabaseScope(connection, transaction);
    }

    internal static ShilpoHubDbContext CreateContext(DbContextOptionsBuilder<ShilpoHubDbContext> builder)
        => new(builder.AddInterceptors(new ProductIndexDirtyInterceptor()).Options);
}
