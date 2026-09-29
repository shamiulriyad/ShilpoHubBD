using ShilpoHubBD.Infrastructure.Security;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Infrastructure;

/// <summary>
/// The runner is pointed at a small stand-in "pg_dump" shell script through Backup:PgDumpPath, which records
/// the arguments and environment it was started with. No real pg_dump or database is needed.
/// </summary>
[Trait("Feature", "Security")]
[Trait("Layer", "Infrastructure")]
public sealed class PgDumpBackupRunnerTests : IDisposable
{
    private const string ConnectionString = "Host=db.internal;Port=5433;Database=shilpohub;Username=backup_user;Password=s3cret;SSL Mode=Require";

    private readonly string _workDirectory = Path.Combine(Path.GetTempPath(), "shilpohub-pgdump-tests-" + Guid.NewGuid().ToString("N"));

    public PgDumpBackupRunnerTests() => Directory.CreateDirectory(_workDirectory);

    public void Dispose() => Directory.Delete(_workDirectory, recursive: true);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string OutputFile => Path.Combine(_workDirectory, "backup.sql");
    private string ArgsFile => Path.Combine(_workDirectory, "args.txt");
    private string EnvFile => Path.Combine(_workDirectory, "env.txt");

    /// <summary>Writes a stand-in pg_dump that records its input, runs <paramref name="body"/>, then exits.</summary>
    private string FakePgDump(string body = "")
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The stand-in pg_dump is a POSIX shell script.");
        var path = Path.Combine(_workDirectory, "pg_dump");
        File.WriteAllText(path,
            "#!/bin/sh\n"
            + $"printf '%s\\n' \"$@\" > '{ArgsFile}'\n"
            + $"printf 'PGPASSWORD=%s\\nPGSSLMODE=%s\\n' \"$PGPASSWORD\" \"$PGSSLMODE\" > '{EnvFile}'\n"
            + body + "\n");
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return path;
    }

    private const string WriteDumpFile = "for a in \"$@\"; do case \"$a\" in --file=*) printf 'dump-content' > \"${a#--file=}\";; esac; done\nexit 0";

    private static PgDumpBackupRunner Runner(string? connectionString, string? pgDumpPath)
        => new(TestConfiguration.From(("ConnectionStrings:DefaultConnection", connectionString), ("Backup:PgDumpPath", pgDumpPath)));

    // ---------- configuration problems ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RunAsync_NoConnectionString_FailsWithAClearMessage(string? connectionString)
    {
        var result = await Runner(connectionString, "/unused").RunAsync(OutputFile, Ct);

        Assert.False(result.Success);
        Assert.Null(result.FileSizeBytes);
        Assert.Equal("No 'DefaultConnection' connection string is configured.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("Database=shilpohub;Username=u")]
    [InlineData("Host=db.internal;Username=u")]
    public async Task RunAsync_ConnectionStringWithoutHostOrDatabase_FailsWithAClearMessage(string connectionString)
    {
        var result = await Runner(connectionString, "/unused").RunAsync(OutputFile, Ct);

        Assert.False(result.Success);
        Assert.Equal("Connection string is missing Host or Database.", result.ErrorMessage);
    }

    [Fact]
    public async Task RunAsync_PgDumpNotInstalled_FailsExplainingHowToFixIt()
    {
        var missing = Path.Combine(_workDirectory, "no-such-pg_dump");

        var result = await Runner(ConnectionString, missing).RunAsync(OutputFile, Ct);

        Assert.False(result.Success);
        Assert.StartsWith($"Could not run pg_dump ('{missing}'). Install PostgreSQL client tools or set 'Backup:PgDumpPath'", result.ErrorMessage);
    }

    // ---------- running pg_dump ----------

    [Fact]
    public async Task RunAsync_PgDumpSucceeds_ReportsSuccessWithTheDumpFileSize()
    {
        var result = await Runner(ConnectionString, FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("dump-content".Length, result.FileSizeBytes);
    }

    [Fact]
    public async Task RunAsync_PassesTheConnectionDetailsAsPgDumpArguments()
    {
        await Runner(ConnectionString, FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        Assert.Equal(
            new[]
            {
                "--host=db.internal", "--port=5433", "--username=backup_user", "--dbname=shilpohub",
                "--no-password", "--format=plain", $"--file={OutputFile}",
            },
            await File.ReadAllLinesAsync(ArgsFile, Ct));
    }

    [Fact]
    public async Task RunAsync_PassesThePasswordAndSslModeThroughTheEnvironmentNotTheArguments()
    {
        await Runner(ConnectionString, FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        Assert.Equal(new[] { "PGPASSWORD=s3cret", "PGSSLMODE=require" }, await File.ReadAllLinesAsync(EnvFile, Ct));
        Assert.DoesNotContain(await File.ReadAllLinesAsync(ArgsFile, Ct), a => a.Contains("s3cret", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_MissingPortAndUser_DefaultsTo5432AndPostgres()
    {
        await Runner("Host=db.internal;Database=shilpohub", FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        var args = await File.ReadAllLinesAsync(ArgsFile, Ct);
        Assert.Contains("--port=5432", args);
        Assert.Contains("--username=postgres", args);
        Assert.Contains("PGPASSWORD=", await File.ReadAllLinesAsync(EnvFile, Ct));
    }

    [Fact]
    public async Task RunAsync_UserIdKeyAndMixedCaseKeys_AreUnderstood()
    {
        await Runner("HOST=db.internal;DataBase=shilpohub;User Id=legacy_user", FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        var args = await File.ReadAllLinesAsync(ArgsFile, Ct);
        Assert.Contains("--host=db.internal", args);
        Assert.Contains("--dbname=shilpohub", args);
        Assert.Contains("--username=legacy_user", args);
    }

    [Theory]
    [InlineData("SSL Mode=Require", "require")]
    [InlineData("SSL Mode=true", "require")]
    [InlineData("SSL Mode=Disable", "disable")]
    [InlineData("SslMode=false", "disable")]
    [InlineData("SSL Mode=Prefer", "prefer")]
    [InlineData("SSL Mode=VerifyFull", "prefer")]
    [InlineData("", "prefer")]
    public async Task RunAsync_MapsTheSslModeToPgSslMode(string sslSetting, string expected)
    {
        await Runner($"Host=db.internal;Database=shilpohub;{sslSetting}", FakePgDump(WriteDumpFile)).RunAsync(OutputFile, Ct);

        Assert.Contains($"PGSSLMODE={expected}", await File.ReadAllLinesAsync(EnvFile, Ct));
    }

    [Fact]
    public async Task RunAsync_PgDumpExitsWithAnError_FailsWithTheExitCodeAndItsErrorOutput()
    {
        var result = await Runner(ConnectionString, FakePgDump("echo 'connection refused' >&2\nexit 3")).RunAsync(OutputFile, Ct);

        Assert.False(result.Success);
        Assert.Null(result.FileSizeBytes);
        Assert.StartsWith("pg_dump exited with code 3: ", result.ErrorMessage);
        Assert.Contains("connection refused", result.ErrorMessage);
    }

    [Fact]
    public async Task RunAsync_PgDumpSucceedsWithoutWritingAFile_ReportsSuccessWithUnknownSize()
    {
        var result = await Runner(ConnectionString, FakePgDump("exit 0")).RunAsync(OutputFile, Ct);

        Assert.True(result.Success);
        Assert.Null(result.FileSizeBytes);
    }
}
