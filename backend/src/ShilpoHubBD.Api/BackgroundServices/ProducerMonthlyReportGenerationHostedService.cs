using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.BackgroundServices;

/// <summary>
/// Generates last calendar month's ProducerMonthlyReport snapshots automatically, once a day, so the
/// "October 1 -> generate September" turnover doesn't depend on an admin remembering to click Generate.
///
/// Runs once immediately on startup (so a restart after a missed day catches up right away) and then
/// once every 24 hours. Idempotent and safe to run repeatedly by construction: it only ever targets
/// "the month before the current one", and GenerateForMonthAsync itself skips any producer who already
/// has a report for that month — it can never touch or duplicate an existing (frozen) report.
///
/// If the process is down across an entire month boundary (e.g. October never gets a tick), the
/// automatic job will simply move on to targeting November once it's back — it does not itself replay
/// every missed month. That gap is exactly what Admin's manual "generate for month" endpoint
/// (POST /api/admin/producer-monthly-reports/generate) exists to backfill; this job does not need to
/// re-implement that as an automatic catch-up.
///
/// Single-instance assumption: this does not use a distributed lock. If this API is ever run as
/// multiple concurrent replicas, more than one instance could attempt the same month at once — the
/// unique (ProducerId, Year, Month) index prevents a duplicate row, but a losing instance would see a
/// DbUpdateException for that one producer. Not a concern for the current single-instance deployment.
/// </summary>
public class ProducerMonthlyReportGenerationHostedService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProducerMonthlyReportGenerationHostedService> _logger;

    public ProducerMonthlyReportGenerationHostedService(
        IServiceScopeFactory scopeFactory, ILogger<ProducerMonthlyReportGenerationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        // Run once immediately on startup (so a restart catches up right away), then once per tick.
        await RunOnceAsync(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var previousMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IProducerMonthlyReportService>();

            var result = await reportService.GenerateForMonthAsync(previousMonth.Year, previousMonth.Month, cancellationToken);

            _logger.LogInformation(
                "Automatic Producer monthly report generation for {Year}-{Month:D2} completed: {Generated} generated, {Skipped} already existed, {Total} producers total.",
                result.Year, result.Month, result.GeneratedCount, result.SkippedCount, result.ProducerCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown mid-run — not a failure.
        }
        catch (Exception ex)
        {
            // Never let a failed run take the hosted service down — the next scheduled tick (or a
            // manual admin regenerate in the meantime) gets another chance at this same month.
            _logger.LogError(
                ex, "Automatic Producer monthly report generation for {Year}-{Month:D2} failed.",
                previousMonth.Year, previousMonth.Month);
        }
    }
}
