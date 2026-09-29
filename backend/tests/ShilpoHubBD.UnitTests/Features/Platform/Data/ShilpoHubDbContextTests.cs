using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Platform.Data;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Platform")]
[Trait("Layer", "Data")]
public class ShilpoHubDbContextTests
{
    private readonly TestDatabaseFixture _database;

    public ShilpoHubDbContextTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Model_BuildsWithoutErrors()
    {
        var context = new ShilpoHubDbContext(new DbContextOptionsBuilder<ShilpoHubDbContext>()
            .UseNpgsql("Host=localhost;Database=shilpohub_model_check;Username=postgres;Password=postgres")
            .Options);

        var model = context.Model;

        Assert.NotEmpty(model.GetEntityTypes());
    }

    [Fact]
    public void Model_EveryEntityWithASingleGuidKey_HasValueGeneratedNever()
    {
        // Application code always assigns its own Guid keys (or relies on AssignMissingGuidKeys), so a
        // regression back to EF's default ValueGeneratedOnAdd would turn some inserts into no-op
        // UPDATEs and fail with DbUpdateConcurrencyException -- the historical "CSR Approve" bug.
        var context = new ShilpoHubDbContext(new DbContextOptionsBuilder<ShilpoHubDbContext>()
            .UseNpgsql("Host=localhost;Database=shilpohub_model_check;Username=postgres;Password=postgres")
            .Options);

        var singleGuidKeyEntities = context.Model.GetEntityTypes()
            .Where(e => !e.IsOwned())
            .Select(e => (Entity: e, Key: e.FindPrimaryKey()))
            .Where(x => x.Key is { Properties.Count: 1 } && x.Key.Properties[0].ClrType == typeof(Guid))
            .ToList();

        Assert.NotEmpty(singleGuidKeyEntities);
        Assert.All(singleGuidKeyEntities, x =>
            Assert.Equal(ValueGenerated.Never, x.Key!.Properties[0].ValueGenerated));
    }

    [Fact]
    public async Task SaveChanges_NewEntityWithAPreAssignedGuidKey_IsInsertedNotUpdated()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var log = new AuditLog { Id = Guid.NewGuid(), ActorName = "System", Action = "x", EntityType = "x", Description = "x", CreatedAt = DateTime.UtcNow };

        context.AuditLogs.Add(log);
        context.SaveChanges();

        Assert.True(await db.NewContext().AuditLogs.AnyAsync(l => l.Id == log.Id, Ct));
    }

    [Fact]
    [Trait("Needs", "Database")]
    public async Task SaveChanges_NewEntityWithAnEmptyGuidKey_IsAssignedAFreshId()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var log = new AuditLog { Id = Guid.Empty, ActorName = "System", Action = "x", EntityType = "x", Description = "x", CreatedAt = DateTime.UtcNow };
        context.AuditLogs.Add(log);

        context.SaveChanges();

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.True(await db.NewContext().AuditLogs.AnyAsync(l => l.Id == log.Id, Ct));
    }
}
