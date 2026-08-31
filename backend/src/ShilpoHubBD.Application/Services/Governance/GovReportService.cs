using System.Text.Json;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO reporting: generates period reports that assemble live figures from the
/// dashboard, monitoring, funding and research modules into stored, self-contained sections; serves
/// the district-keyed GIS payload; and records downloadable-analytics export requests (metadata only,
/// files produced by an external worker).
/// </summary>
public class GovReportService : IGovReportService
{
    private static readonly string[] GisMetrics =
        { "producers", "products", "villages", "orders", "sales", "risk" };

    private readonly IGovAnalyticsRepository _repository;

    public GovReportService(IGovAnalyticsRepository repository)
    {
        _repository = repository;
    }

    // ==== Reports ====================================================

    public async Task<GovReportDto> GenerateAsync(
        Guid userId, GenerateGovReportRequest request, CancellationToken cancellationToken)
    {
        var type = ParseEnum<GovReportType>(request.ReportType, "Invalid ReportType.");
        var (from, to) = ResolvePeriod(type, request.PeriodStart, request.PeriodEnd);

        var data = await _repository.GatherReportDataAsync(from, to, cancellationToken);
        var now = DateTime.UtcNow;

        var report = new GovReport
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            ReportType = type,
            Status = request.Publish ? GovReportStatus.Published : GovReportStatus.Draft,
            PeriodStart = from,
            PeriodEnd = to,
            Highlights = request.Highlights?.Trim(),
            Summary = BuildSummary(data, from, to),
            PayloadJson = JsonSerializer.Serialize(data),
            GeneratedAt = now,
            GeneratedByUserId = userId,
            PublishedAt = request.Publish ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        AddSection(report, "economy", "Heritage economy", 0,
            $"Marketplace sales of {data.MarketplaceSalesValue:N0} across {data.Orders} orders, "
            + $"export channel {data.ExportSalesValue:N0}, tourism revenue {data.TourismRevenue:N0}.",
            new
            {
                data.MarketplaceSalesValue,
                data.ExportSalesValue,
                data.TourismRevenue,
                data.Orders,
                data.TourismBookings,
                heritageEconomyValue = data.MarketplaceSalesValue + data.TourismRevenue,
            });

        AddSection(report, "producers-employment", "Producers & employment", 1,
            $"{data.ActiveProducers} active producers ({data.NewProducers} new this period); "
            + $"{data.JobsPosted} jobs posted, {data.JobsFilled} filled.",
            new
            {
                data.TotalProducers,
                data.ActiveProducers,
                data.NewProducers,
                data.JobsPosted,
                data.JobsFilled,
                data.DistrictsCovered,
                data.Villages,
            });

        AddSection(report, "monitoring", "Monitoring & integrity", 2,
            $"{data.FlagsRaised} flags raised this period, {data.FlagsOpen} currently open; "
            + $"{data.ComplaintsReceived} complaints received, {data.ComplaintsResolved} resolved.",
            new
            {
                data.FlagsRaised,
                data.FlagsOpen,
                flagsByType = data.FlagsByType,
                data.ComplaintsReceived,
                data.ComplaintsResolved,
            });

        AddSection(report, "funding", "Funding pipeline", 3,
            $"{data.FundingApplicationsSubmitted} applications submitted, {data.FundingApplicationsApproved} "
            + $"approved for {data.FundingApproved:N0}; {data.FundingDisbursed:N0} disbursed.",
            new
            {
                data.FundingProgramsActive,
                data.FundingApplicationsSubmitted,
                data.FundingApplicationsApproved,
                data.FundingApproved,
                data.FundingDisbursed,
            });

        AddSection(report, "research", "Research & policy activity", 4,
            $"{data.PolicySimulationsRun} policy simulations run and {data.HeritageIndicesComputed} "
            + "heritage-intelligence indices computed this period.",
            new { data.PolicySimulationsRun, data.HeritageIndicesComputed });

        await _repository.AddReportAsync(report, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetReportByIdAsync(report.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<GovReportListItemDto>> GetReportsAsync(
        GovReportQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetReportsPagedAsync(query, cancellationToken);

        return new PagedResult<GovReportListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<GovReportDto> GetReportByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadReportAsync(id, cancellationToken)).ToDto();

    public async Task<GovReportDto> UpdateReportAsync(
        Guid userId, Guid id, UpdateGovReportRequest request, CancellationToken cancellationToken)
    {
        var report = await LoadReportAsync(id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            report.Title = request.Title.Trim();
        }

        if (request.Highlights is not null)
        {
            report.Highlights = request.Highlights.Trim();
        }

        if (request.Summary is not null)
        {
            report.Summary = request.Summary.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = ParseEnum<GovReportStatus>(request.Status, "Invalid Status.");
            if (status == GovReportStatus.Published && report.PublishedAt is null)
            {
                report.PublishedAt = DateTime.UtcNow;
            }

            report.Status = status;
        }

        report.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetReportByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteReportAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await LoadReportAsync(id, cancellationToken);
        _repository.RemoveReport(report);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ==== GIS ======================================================

    public async Task<GisMapDto> GetGisMapAsync(GisMapQueryParameters query, CancellationToken cancellationToken)
    {
        var metric = (query.Metric ?? "sales").Trim().ToLowerInvariant();
        if (!GisMetrics.Contains(metric))
        {
            throw new ConflictException($"Metric must be one of: {string.Join(", ", GisMetrics)}.");
        }

        var from = query.From.HasValue ? DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc) : (DateTime?)null;
        var to = query.To.HasValue ? DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc) : (DateTime?)null;
        if (from.HasValue && to.HasValue && to <= from)
        {
            throw new ConflictException("'to' must be after 'from'.");
        }

        var rows = await _repository.GetGisDistrictValuesAsync(metric, from, to, cancellationToken);
        var ordered = rows.OrderByDescending(r => r.Value).ThenBy(r => r.Name).ToList();

        return new GisMapDto
        {
            GeneratedAt = DateTime.UtcNow,
            Metric = metric,
            FromDate = from,
            ToDate = to,
            MinValue = rows.Count == 0 ? 0 : rows.Min(r => r.Value),
            MaxValue = rows.Count == 0 ? 0 : rows.Max(r => r.Value),
            Districts = ordered.Select((r, i) => new GisDistrictPointDto
            {
                DistrictId = r.DistrictId,
                Name = r.Name,
                Division = r.Division,
                Value = r.Value,
                Rank = i + 1,
            }).ToList(),
        };
    }

    // ==== Exports ==================================================

    public async Task<AnalyticsExportDto> RequestExportAsync(
        Guid userId, CreateAnalyticsExportRequest request, CancellationToken cancellationToken)
    {
        var dataset = ParseEnum<AnalyticsExportDataset>(request.Dataset, "Invalid Dataset.");
        var format = ParseEnum<AnalyticsExportFormat>(request.Format, "Invalid Format.");

        if (dataset == AnalyticsExportDataset.GovReport)
        {
            if (request.GovReportId is not { } reportId)
            {
                throw new ConflictException("GovReportId is required when Dataset is GovReport.");
            }

            if (!await _repository.ReportExistsAsync(reportId, cancellationToken))
            {
                throw new NotFoundException("Report not found.");
            }
        }

        var export = new AnalyticsExport
        {
            Id = Guid.NewGuid(),
            Dataset = dataset,
            Format = format,
            Status = AnalyticsExportStatus.Pending,
            FiltersJson = string.IsNullOrWhiteSpace(request.FiltersJson) ? null : request.FiltersJson.Trim(),
            GovReportId = dataset == AnalyticsExportDataset.GovReport ? request.GovReportId : null,
            RequestedByUserId = userId,
            RequestedAt = DateTime.UtcNow,
        };

        await _repository.AddExportAsync(export, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetExportByIdAsync(export.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<AnalyticsExportDto>> GetExportsAsync(
        Guid userId, AnalyticsExportQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetExportsPagedAsync(query, userId, cancellationToken);

        return new PagedResult<AnalyticsExportDto>
        {
            Items = items.Select(e => e.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<AnalyticsExportDto> GetExportByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadExportAsync(id, cancellationToken)).ToDto();

    public async Task<AnalyticsExportDto> CompleteExportAsync(
        Guid userId, Guid id, CompleteAnalyticsExportRequest request, CancellationToken cancellationToken)
    {
        var export = await LoadExportAsync(id, cancellationToken);
        if (export.Status is AnalyticsExportStatus.Completed or AnalyticsExportStatus.Failed)
        {
            throw new ConflictException($"Export is already {export.Status}.");
        }

        var outcome = ParseEnum<AnalyticsExportStatus>(request.Outcome, "Outcome must be Completed or Failed.");
        if (outcome is not (AnalyticsExportStatus.Completed or AnalyticsExportStatus.Failed))
        {
            throw new ConflictException("Outcome must be Completed or Failed.");
        }

        export.Status = outcome;
        export.CompletedAt = DateTime.UtcNow;
        if (outcome == AnalyticsExportStatus.Completed)
        {
            export.FileUrl = request.FileUrl?.Trim();
            export.RowCount = request.RowCount;
            export.FileSizeBytes = request.FileSizeBytes;
        }
        else
        {
            export.FailureReason = request.FailureReason?.Trim() ?? "Export failed.";
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetExportByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteExportAsync(Guid id, CancellationToken cancellationToken)
    {
        var export = await LoadExportAsync(id, cancellationToken);
        _repository.RemoveExport(export);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ==== helpers ==================================================

    private async Task<GovReport> LoadReportAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetReportByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Report not found.");

    private async Task<AnalyticsExport> LoadExportAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetExportByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Analytics export not found.");

    private static (DateTime From, DateTime To) ResolvePeriod(
        GovReportType type, DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue)
        {
            var f = DateTime.SpecifyKind(start.Value, DateTimeKind.Utc);
            var t = DateTime.SpecifyKind(end.Value, DateTimeKind.Utc);
            if (t <= f)
            {
                throw new ConflictException("PeriodEnd must be after PeriodStart.");
            }

            return (f, t);
        }

        var now = DateTime.UtcNow;
        return type switch
        {
            GovReportType.Monthly => (
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
            GovReportType.Quarterly => (now.AddMonths(-3), now),
            GovReportType.Annual => (now.AddMonths(-12), now),
            _ => (now.AddMonths(-1), now),
        };
    }

    private static void AddSection(
        GovReport report, string key, string title, int order, string narrative, object content)
        => report.Sections.Add(new GovReportSection
        {
            Id = Guid.NewGuid(),
            GovReportId = report.Id,
            Key = key,
            Title = title,
            Narrative = narrative,
            ContentJson = JsonSerializer.Serialize(content),
            DisplayOrder = order,
        });

    private static string BuildSummary(GovReportData d, DateTime from, DateTime to)
        => $"Period {from:yyyy-MM-dd} to {to:yyyy-MM-dd}: heritage economy "
            + $"{d.MarketplaceSalesValue + d.TourismRevenue:N0} (marketplace {d.MarketplaceSalesValue:N0}, "
            + $"tourism {d.TourismRevenue:N0}), {d.ActiveProducers} active producers, {d.JobsFilled} jobs filled, "
            + $"{d.FlagsRaised} monitoring flags raised, {d.FundingApproved:N0} funding approved.";

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
