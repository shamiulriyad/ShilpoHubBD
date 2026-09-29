using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Security.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Security")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class BackupRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public BackupRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static BackupRecord Record(User requester, DateTime startedAt, BackupStatus status = BackupStatus.Completed) => new()
    {
        Id = Guid.NewGuid(),
        RequestedByUserId = requester.Id,
        Status = status,
        FilePath = status == BackupStatus.Completed ? $"/backups/{Guid.NewGuid():N}.sql" : null,
        StartedAt = startedAt,
    };

    private static async Task<(User Requester, List<BackupRecord> Records)> SeedAsync(ShilpoHubDbContext context, params DateTime[] startTimes)
    {
        var requester = TestUsers.Create(fullName: "Admin Person");
        var records = startTimes.Select(t => Record(requester, t)).ToList();
        context.Users.Add(requester);
        context.BackupRecords.AddRange(records);
        await context.SaveChangesAsync(Ct);
        return (requester, records);
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheStatusAsText()
    {
        await using var db = await _database.BeginAsync();
        var (requester, _) = await SeedAsync(db.NewContext());
        var record = Record(requester, DateTime.UtcNow, BackupStatus.Failed);
        record.ErrorMessage = "pg_dump missing";
        var repository = new BackupRepository(db.NewContext());

        await repository.AddAsync(record, Ct);
        await repository.SaveChangesAsync(Ct);

        var stored = await db.NewContext().BackupRecords.SingleAsync(b => b.Id == record.Id, Ct);
        Assert.Equal(BackupStatus.Failed, stored.Status);
        Assert.Equal("pg_dump missing", stored.ErrorMessage);
        var context = db.NewContext();
        var raw = await context.Database
            .SqlQuery<string>($"SELECT \"Status\" AS \"Value\" FROM \"BackupRecords\" WHERE \"Id\" = {record.Id}")
            .SingleAsync(Ct);
        Assert.Equal("Failed", raw);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsTheRequester()
    {
        await using var db = await _database.BeginAsync();
        var (_, records) = await SeedAsync(db.NewContext(), DateTime.UtcNow);

        var found = await new BackupRepository(db.NewContext()).GetByIdAsync(records[0].Id, Ct);

        Assert.NotNull(found);
        Assert.Equal("Admin Person", found.RequestedBy.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new BackupRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMostRecentlyStartedFirstWithTotal()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var (_, records) = await SeedAsync(db.NewContext(), now.AddHours(-2), now, now.AddHours(-1));

        var (items, total) = await new BackupRepository(db.NewContext()).GetPagedAsync(1, 10, Ct);

        Assert.Equal(3, total);
        Assert.Equal(new[] { records[1].Id, records[2].Id, records[0].Id }, items.Select(b => b.Id));
        Assert.All(items, b => Assert.Equal("Admin Person", b.RequestedBy.FullName));
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_SkipsTheFirstPage()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var (_, records) = await SeedAsync(db.NewContext(), now, now.AddHours(-1), now.AddHours(-2));

        var (items, total) = await new BackupRepository(db.NewContext()).GetPagedAsync(2, 2, Ct);

        Assert.Equal(3, total);
        Assert.Equal(records[2].Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task Remove_ThenSaveChangesAsync_DeletesTheRecord()
    {
        await using var db = await _database.BeginAsync();
        var (_, records) = await SeedAsync(db.NewContext(), DateTime.UtcNow);
        var repository = new BackupRepository(db.NewContext());
        var record = await repository.GetByIdAsync(records[0].Id, Ct);

        repository.Remove(record!);
        await repository.SaveChangesAsync(Ct);

        Assert.False(await db.NewContext().BackupRecords.AnyAsync(b => b.Id == records[0].Id, Ct));
    }
}
