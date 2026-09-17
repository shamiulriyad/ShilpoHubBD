using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.Security;

/// <summary>
/// Triggers a real <c>pg_dump</c> against the configured Postgres connection string. Genuinely runs the
/// dump when PostgreSQL client tools are available (on PATH, or at the path in "Backup:PgDumpPath"); if
/// they are not, it fails clearly with a diagnosable message rather than pretending to succeed.
/// </summary>
public class PgDumpBackupRunner : IBackupRunner
{
    private readonly IConfiguration _configuration;

    public PgDumpBackupRunner(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<BackupRunResult> RunAsync(string outputFilePath, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new BackupRunResult(false, null, "No 'DefaultConnection' connection string is configured.");
        }

        var parts = ParseConnectionString(connectionString);
        if (!parts.TryGetValue("host", out var host) || !parts.TryGetValue("database", out var database))
        {
            return new BackupRunResult(false, null, "Connection string is missing Host or Database.");
        }

        var port = parts.GetValueOrDefault("port", "5432");
        var username = parts.GetValueOrDefault("username", parts.GetValueOrDefault("user id", "postgres"));
        var password = parts.GetValueOrDefault("password", string.Empty);
        var sslMode = parts.GetValueOrDefault("ssl mode", parts.GetValueOrDefault("sslmode", "Prefer"));

        var pgDumpPath = _configuration["Backup:PgDumpPath"] ?? "pg_dump";

        var startInfo = new ProcessStartInfo
        {
            FileName = pgDumpPath,
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add($"--host={host}");
        startInfo.ArgumentList.Add($"--port={port}");
        startInfo.ArgumentList.Add($"--username={username}");
        startInfo.ArgumentList.Add($"--dbname={database}");
        startInfo.ArgumentList.Add("--no-password");
        startInfo.ArgumentList.Add("--format=plain");
        startInfo.ArgumentList.Add($"--file={outputFilePath}");
        startInfo.Environment["PGPASSWORD"] = password;
        startInfo.Environment["PGSSLMODE"] = sslMode.ToLowerInvariant() switch
        {
            "require" or "true" => "require",
            "disable" or "false" => "disable",
            _ => "prefer",
        };

        try
        {
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("pg_dump process could not be started.");

            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                return new BackupRunResult(false, null, $"pg_dump exited with code {process.ExitCode}: {stderr}");
            }

            var size = File.Exists(outputFilePath) ? new FileInfo(outputFilePath).Length : (long?)null;
            return new BackupRunResult(true, size, null);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return new BackupRunResult(
                false, null,
                $"Could not run pg_dump ('{pgDumpPath}'). Install PostgreSQL client tools or set "
                + $"'Backup:PgDumpPath' in configuration. Details: {ex.Message}");
        }
    }

    private static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = segment.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            var key = segment[..eq].Trim().ToLowerInvariant();
            var value = segment[(eq + 1)..].Trim();
            result[key] = value;
        }

        return result;
    }
}
