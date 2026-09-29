using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShilpoHubBD.Data;

namespace ShilpoHubBD.UnitTests.Common.Database;

/// <summary>
/// One open connection and transaction shared by every context the test creates, so a fresh context
/// can read what another one saved. Disposing rolls the transaction back and leaves no rows behind.
/// </summary>
public sealed class DatabaseScope : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;
    private readonly List<ShilpoHubDbContext> _contexts = new();

    internal DatabaseScope(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public ShilpoHubDbContext NewContext()
    {
        var context = TestDatabaseFixture.CreateContext(new DbContextOptionsBuilder<ShilpoHubDbContext>().UseNpgsql(_connection));
        context.Database.UseTransaction(_transaction);
        _contexts.Add(context);
        return context;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var context in _contexts)
        {
            await context.DisposeAsync();
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
