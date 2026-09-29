using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Security;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Services;

[Trait("Feature", "Security")]
[Trait("Layer", "Service")]
public sealed class BackupServiceTests : IDisposable
{
    private readonly IBackupRepository _backups = Substitute.For<IBackupRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IBackupRunner _runner = Substitute.For<IBackupRunner>();
    private readonly string _backupDirectory = Path.Combine(Path.GetTempPath(), "shilpohub-backup-tests-" + Guid.NewGuid().ToString("N"));
    private readonly User _admin = TestUsers.Create(fullName: "Admin Person");

    public BackupServiceTests()
    {
        _users.GetByIdAsync(_admin.Id, Arg.Any<CancellationToken>()).Returns(_admin);
    }

    public void Dispose()
    {
        if (Directory.Exists(_backupDirectory))
        {
            Directory.Delete(_backupDirectory, recursive: true);
        }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private BackupService CreateService()
        => new(_backups, _users, _runner, TestConfiguration.From(("Backup:Directory", _backupDirectory)));

    private BackupRecord StoredRecord(string? filePath = null)
    {
        var record = new BackupRecord
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = _admin.Id,
            RequestedBy = _admin,
            Status = BackupStatus.Completed,
            FilePath = filePath,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-4),
        };
        _backups.GetByIdAsync(record.Id, Arg.Any<CancellationToken>()).Returns(record);
        return record;
    }

    // ---------- TriggerBackupAsync ----------

    [Fact]
    public async Task TriggerBackupAsync_RunnerSucceeds_RecordsACompletedBackupWithFileAndSize()
    {
        BackupRecord? saved = null;
        await _backups.AddAsync(Arg.Do<BackupRecord>(r => saved = r), Arg.Any<CancellationToken>());
        _runner.RunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new BackupRunResult(true, 2048, null));
        var before = DateTime.UtcNow;

        var dto = await CreateService().TriggerBackupAsync(_admin.Id, Ct);

        Assert.NotNull(saved);
        Assert.Equal(BackupStatus.Completed, saved.Status);
        Assert.Equal(2048, saved.FileSizeBytes);
        Assert.Null(saved.ErrorMessage);
        Assert.NotNull(saved.FilePath);
        Assert.Equal(_backupDirectory, Path.GetDirectoryName(saved.FilePath));
        Assert.Matches(@"^backup-\d{8}-\d{6}\.sql$", Path.GetFileName(saved.FilePath));
        Assert.InRange(saved.StartedAt, before, DateTime.UtcNow);
        Assert.NotNull(saved.CompletedAt);
        Assert.Equal(_admin.Id, saved.RequestedByUserId);
        Assert.Equal(BackupStatus.Completed, dto.Status);
        Assert.Equal("Admin Person", dto.RequestedByName);
        Assert.Equal(saved.FilePath, dto.FilePath);
        await _runner.Received(1).RunAsync(saved.FilePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerBackupAsync_SavesARunningRecordBeforeTheDumpStartsAndSavesAgainAfter()
    {
        BackupRecord? saved = null;
        await _backups.AddAsync(Arg.Do<BackupRecord>(r => saved = r), Arg.Any<CancellationToken>());
        var statusWhileRunning = (BackupStatus?)null;
        var savesBeforeRun = -1;
        _runner.RunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            statusWhileRunning = saved?.Status;
            savesBeforeRun = _backups.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IBackupRepository.SaveChangesAsync));
            return new BackupRunResult(true, 1, null);
        });

        await CreateService().TriggerBackupAsync(_admin.Id, Ct);

        Assert.Equal(BackupStatus.Running, statusWhileRunning);
        Assert.Equal(1, savesBeforeRun);
        await _backups.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerBackupAsync_RunnerFails_RecordsAFailedBackupWithTheErrorAndNoFile()
    {
        BackupRecord? saved = null;
        await _backups.AddAsync(Arg.Do<BackupRecord>(r => saved = r), Arg.Any<CancellationToken>());
        _runner.RunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new BackupRunResult(false, null, "Could not run pg_dump."));

        var dto = await CreateService().TriggerBackupAsync(_admin.Id, Ct);

        Assert.NotNull(saved);
        Assert.Equal(BackupStatus.Failed, saved.Status);
        Assert.Null(saved.FilePath);
        Assert.Null(saved.FileSizeBytes);
        Assert.Equal("Could not run pg_dump.", saved.ErrorMessage);
        Assert.NotNull(saved.CompletedAt);
        Assert.Equal(BackupStatus.Failed, dto.Status);
        Assert.Equal("Could not run pg_dump.", dto.ErrorMessage);
    }

    [Fact]
    public async Task TriggerBackupAsync_CreatesTheBackupDirectoryWhenMissing()
    {
        _runner.RunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new BackupRunResult(true, 1, null));
        Assert.False(Directory.Exists(_backupDirectory));

        await CreateService().TriggerBackupAsync(_admin.Id, Ct);

        Assert.True(Directory.Exists(_backupDirectory));
    }

    [Fact]
    public async Task TriggerBackupAsync_NoConfiguredDirectory_UsesBackupsUnderTheAppFolder()
    {
        var defaultDirectory = Path.Combine(AppContext.BaseDirectory, "backups");
        var existedBefore = Directory.Exists(defaultDirectory);
        string? usedPath = null;
        _runner.RunAsync(Arg.Do<string>(p => usedPath = p), Arg.Any<CancellationToken>()).Returns(new BackupRunResult(false, null, "x"));

        try
        {
            await new BackupService(_backups, _users, _runner, TestConfiguration.Empty()).TriggerBackupAsync(_admin.Id, Ct);

            Assert.Equal(defaultDirectory, Path.GetDirectoryName(usedPath));
        }
        finally
        {
            if (!existedBefore && Directory.Exists(defaultDirectory))
            {
                Directory.Delete(defaultDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task TriggerBackupAsync_UnknownUser_ThrowsNotFoundWithoutRecordingOrRunning()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().TriggerBackupAsync(Guid.NewGuid(), Ct));

        Assert.Equal("User not found.", error.Message);
        await _backups.DidNotReceive().AddAsync(Arg.Any<BackupRecord>(), Arg.Any<CancellationToken>());
        await _runner.DidNotReceive().RunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.False(Directory.Exists(_backupDirectory));
    }

    // ---------- GetPagedAsync / GetByIdAsync ----------

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(3, 0, 3, 20)]
    [InlineData(3, 101, 3, 20)]
    [InlineData(2, 100, 2, 100)]
    public async Task GetPagedAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _backups.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<BackupRecord>(), 0));

        var result = await CreateService().GetPagedAsync(page, pageSize, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
        await _backups.Received(1).GetPagedAsync(expectedPage, expectedSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_MapsRecordsWithTheRequestersName()
    {
        var record = StoredRecord("/backups/a.sql");
        record.FileSizeBytes = 99;
        _backups.GetPagedAsync(1, 20, Arg.Any<CancellationToken>()).Returns((new List<BackupRecord> { record }, 3));

        var result = await CreateService().GetPagedAsync(1, 20, Ct);

        Assert.Equal(3, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(record.Id, dto.Id);
        Assert.Equal("Admin Person", dto.RequestedByName);
        Assert.Equal(BackupStatus.Completed, dto.Status);
        Assert.Equal("/backups/a.sql", dto.FilePath);
        Assert.Equal(99, dto.FileSizeBytes);
        Assert.Equal(record.StartedAt, dto.StartedAt);
        Assert.Equal(record.CompletedAt, dto.CompletedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRecord_ReturnsIt()
    {
        var record = StoredRecord();

        var dto = await CreateService().GetByIdAsync(record.Id, Ct);

        Assert.Equal(record.Id, dto.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownRecord_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Backup record not found.", error.Message);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_RecordWithFileOnDisk_DeletesTheFileAndTheRecord()
    {
        Directory.CreateDirectory(_backupDirectory);
        var file = Path.Combine(_backupDirectory, "backup-20260101-000000.sql");
        await File.WriteAllTextAsync(file, "-- dump", Ct);
        var record = StoredRecord(file);

        await CreateService().DeleteAsync(record.Id, Ct);

        Assert.False(File.Exists(file));
        _backups.Received(1).Remove(record);
        await _backups.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_FileAlreadyGone_StillDeletesTheRecord()
    {
        var record = StoredRecord(Path.Combine(_backupDirectory, "missing.sql"));

        await CreateService().DeleteAsync(record.Id, Ct);

        _backups.Received(1).Remove(record);
        await _backups.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_FailedBackupWithoutFile_DeletesTheRecord()
    {
        var record = StoredRecord(filePath: null);

        await CreateService().DeleteAsync(record.Id, Ct);

        _backups.Received(1).Remove(record);
    }

    [Fact]
    public async Task DeleteAsync_UnknownRecord_ThrowsNotFoundAndRemovesNothing()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().DeleteAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Backup record not found.", error.Message);
        _backups.DidNotReceive().Remove(Arg.Any<BackupRecord>());
    }
}
