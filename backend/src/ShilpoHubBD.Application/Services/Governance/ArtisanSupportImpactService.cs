using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Application.Services.Governance;

public class ArtisanSupportImpactService : IArtisanSupportImpactService
{
    private readonly IArtisanSupportRepository _repository;
    // Reused purely for its view-permission check (NotFound/Unauthorized) and for ArtisanUserId /
    // SupportProvidedAt — never duplicate that authorization logic here.
    private readonly IArtisanSupportService _caseService;
    private readonly IProducerMonthlyReportRepository _reportRepository;
    private readonly ImpactAssessmentThresholds _thresholds;
    private readonly IProducerImpactAIProvider _aiProvider;

    public ArtisanSupportImpactService(
        IArtisanSupportRepository repository, IArtisanSupportService caseService,
        IProducerMonthlyReportRepository reportRepository, IOptions<ImpactAssessmentThresholds> thresholds,
        IProducerImpactAIProvider aiProvider)
    {
        _repository = repository;
        _caseService = caseService;
        _reportRepository = reportRepository;
        _thresholds = thresholds.Value;
        _aiProvider = aiProvider;
    }

    public async Task<ArtisanSupportImpactAssessmentDto> GenerateAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken)
    {
        var caseDto = await _caseService.GetCaseAsync(userId, caseId, isAdmin, isGovernment, cancellationToken);

        if (caseDto.SupportProvidedAt is null)
        {
            throw new ConflictException("Record support delivery before generating an impact assessment.");
        }

        var (beforeDate, afterDate, beforeReport, afterReport, metrics) =
            await ComputeAsync(caseDto.ArtisanUserId, caseDto.SupportProvidedAt.Value, cancellationToken);

        var assessment = await _repository.GetImpactAssessmentByCaseIdAsync(caseId, cancellationToken);
        if (assessment is null)
        {
            assessment = new ArtisanSupportImpactAssessment { Id = Guid.NewGuid(), CaseId = caseId };
            await _repository.AddImpactAssessmentAsync(assessment, cancellationToken);
        }
        else
        {
            assessment.Metrics.Clear();
        }

        assessment.BeforeYear = beforeDate.Year;
        assessment.BeforeMonth = beforeDate.Month;
        assessment.BeforeReportId = beforeReport?.Id;
        assessment.AfterYear = afterDate.Year;
        assessment.AfterMonth = afterDate.Month;
        assessment.AfterReportId = afterReport?.Id;
        assessment.GeneratedByUserId = userId;
        assessment.GeneratedAt = DateTime.UtcNow;

        foreach (var metric in metrics)
        {
            metric.Id = Guid.NewGuid();
            metric.AssessmentId = assessment.Id;
            assessment.Metrics.Add(metric);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(assessment);
    }

    public async Task<ArtisanSupportImpactAssessmentDto> GetAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken)
    {
        await _caseService.GetCaseAsync(userId, caseId, isAdmin, isGovernment, cancellationToken);

        var assessment = await _repository.GetImpactAssessmentByCaseIdAsync(caseId, cancellationToken)
            ?? throw new NotFoundException("No impact assessment has been generated for this case yet.");

        return ToDto(assessment);
    }

    // Metrics the Producer Support Impact Report asks for — a subset of the full assessment (which
    // also covers UnitsSold/CustomerRetention/ProductActivity).
    private static readonly ImpactMetricType[] ReportMetricTypes =
    {
        ImpactMetricType.Sales, ImpactMetricType.Orders, ImpactMetricType.NetIncome,
        ImpactMetricType.AverageRating, ImpactMetricType.CancellationRate,
    };

    public async Task<List<ProducerSupportImpactReportRowDto>> GetImpactReportAsync(
        Guid userId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken)
    {
        // Same visibility rule GetCasesAsync already applies for case listing — a Producer only ever
        // gets their own cases back, a Government/NGO user only their organization's, so no separate
        // scoping is needed here.
        var cases = await _repository.GetCasesAsync(userId, isAdmin, isGovernment, cancellationToken);
        var completed = cases.Where(c => c.SupportProvidedAt.HasValue && c.SupportKind.HasValue).ToList();

        var organizationNames = (await _repository.GetOrganizationsAsync(cancellationToken))
            .ToDictionary(o => o.UserId, o => o.OrganizationName);

        var rows = new List<ProducerSupportImpactReportRowDto>();

        foreach (var supportCase in completed)
        {
            int beforeYear, beforeMonth, afterYear, afterMonth;
            bool beforeAvailable, afterAvailable;
            List<ArtisanSupportImpactMetric> metrics;

            var persisted = await _repository.GetImpactAssessmentByCaseIdAsync(supportCase.Id, cancellationToken);
            if (persisted is not null)
            {
                beforeYear = persisted.BeforeYear;
                beforeMonth = persisted.BeforeMonth;
                beforeAvailable = persisted.BeforeReportId.HasValue;
                afterYear = persisted.AfterYear;
                afterMonth = persisted.AfterMonth;
                afterAvailable = persisted.AfterReportId.HasValue;
                metrics = persisted.Metrics;
            }
            else
            {
                // No assessment has been explicitly generated for this case yet — compute one live for
                // the report without persisting it, so viewing a report never has a write side effect.
                var (beforeDate, afterDate, beforeReport, afterReport, computed) =
                    await ComputeAsync(supportCase.ArtisanUserId, supportCase.SupportProvidedAt!.Value, cancellationToken);
                beforeYear = beforeDate.Year;
                beforeMonth = beforeDate.Month;
                beforeAvailable = beforeReport is not null;
                afterYear = afterDate.Year;
                afterMonth = afterDate.Month;
                afterAvailable = afterReport is not null;
                metrics = computed;
            }

            rows.Add(new ProducerSupportImpactReportRowDto
            {
                CaseId = supportCase.Id,
                CaseNumber = supportCase.CaseNumber,
                ProducerId = supportCase.ArtisanUserId,
                ProducerName = supportCase.Artisan.FullName,
                SupportOrganizationName = supportCase.OrganizationUserId.HasValue
                    && organizationNames.TryGetValue(supportCase.OrganizationUserId.Value, out var orgName)
                        ? orgName
                        : supportCase.Organization?.FullName ?? "Unassigned",
                SupportType = supportCase.SupportKind!.Value.ToString(),
                SupportDate = supportCase.SupportProvidedAt!.Value,
                BeforeYear = beforeYear,
                BeforeMonth = beforeMonth,
                AfterYear = afterYear,
                AfterMonth = afterMonth,
                MonthsUsedForComparison = (beforeAvailable ? 1 : 0) + (afterAvailable ? 1 : 0),
                // Fixed column order (Sales, Orders, Income, Rating, Cancellation Rate) regardless of
                // whichever order the source list happens to be in — a persisted assessment's Metrics
                // collection has no guaranteed retrieval order once round-tripped through the database.
                Metrics = ReportMetricTypes
                    .Select(type => metrics.FirstOrDefault(m => m.MetricType == type))
                    .Where(m => m is not null)
                    .Select(m => new ArtisanSupportImpactMetricDto
                    {
                        MetricType = m!.MetricType.ToString(),
                        BeforeValue = m.BeforeValue,
                        AfterValue = m.AfterValue,
                        ChangeAbsolute = m.ChangeAbsolute,
                        ChangePercentage = m.ChangePercentage,
                        Status = m.Status.ToString(),
                    }).ToList(),
            });
        }

        return rows.OrderByDescending(r => r.SupportDate).ToList();
    }

    public async Task<ProducerImpactAIAnalysisDto> GenerateAiSummaryAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken)
    {
        var caseDto = await _caseService.GetCaseAsync(userId, caseId, isAdmin, isGovernment, cancellationToken);

        var assessment = await _repository.GetImpactAssessmentByCaseIdAsync(caseId, cancellationToken)
            ?? throw new NotFoundException("Generate the impact assessment for this case before requesting an AI summary.");

        var context = await BuildAiContextAsync(caseDto, assessment, cancellationToken);
        var narrative = await _aiProvider.GenerateNarrativeAsync(context, cancellationToken);

        var analysis = new ProducerImpactAIAnalysis
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            ImpactAssessmentId = assessment.Id,
            RequestedByUserId = userId,
            ProviderName = narrative.ProviderName,
            IsAiGenerated = narrative.IsAiGenerated,
            GeneratedAt = DateTime.UtcNow,
        };

        var order = 0;
        foreach (var finding in narrative.Findings)
        {
            if (!Enum.TryParse<ProducerImpactFindingCategory>(finding.Category, out var category)
                || !Enum.TryParse<ProducerImpactFindingKind>(finding.Kind, out var kind))
            {
                continue;
            }

            analysis.Findings.Add(new ProducerImpactAIFinding
            {
                Id = Guid.NewGuid(),
                AnalysisId = analysis.Id,
                Category = category,
                Kind = kind,
                Text = finding.Text,
                DisplayOrder = order++,
            });
        }

        await _repository.AddAiAnalysisAsync(analysis, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToAiDto(analysis);
    }

    public async Task<ProducerImpactAIAnalysisDto> GetLatestAiSummaryAsync(
        Guid userId, Guid caseId, bool isAdmin, bool isGovernment, CancellationToken cancellationToken)
    {
        await _caseService.GetCaseAsync(userId, caseId, isAdmin, isGovernment, cancellationToken);

        var analysis = await _repository.GetLatestAiAnalysisByCaseIdAsync(caseId, cancellationToken)
            ?? throw new NotFoundException("No AI summary has been generated for this case yet.");

        return ToAiDto(analysis);
    }

    /// <summary>Reshapes already-computed values (the assessment's metrics, the After report's peer positioning) into the AI's input — no new arithmetic happens here.</summary>
    private async Task<ProducerImpactAiContext> BuildAiContextAsync(
        ArtisanSupportCaseDto caseDto, ArtisanSupportImpactAssessment assessment, CancellationToken cancellationToken)
    {
        var organization = caseDto.OrganizationUserId.HasValue
            ? await _repository.GetOrganizationByUserAsync(caseDto.OrganizationUserId.Value, cancellationToken)
            : null;

        var afterReportDetails = await _reportRepository.GetForProducerWithDetailsAsync(
            caseDto.ArtisanUserId, assessment.AfterYear, assessment.AfterMonth, cancellationToken);

        return new ProducerImpactAiContext
        {
            ProducerName = caseDto.ArtisanName,
            SupportOrganizationName = organization?.OrganizationName ?? caseDto.OrganizationName ?? "Unassigned",
            SupportType = caseDto.SupportKind ?? "Unspecified",
            SupportDate = caseDto.SupportProvidedAt ?? DateTime.UtcNow,
            BeforeYear = assessment.BeforeYear,
            BeforeMonth = assessment.BeforeMonth,
            AfterYear = assessment.AfterYear,
            AfterMonth = assessment.AfterMonth,
            MonthsUsedForComparison = (assessment.BeforeReportId.HasValue ? 1 : 0) + (assessment.AfterReportId.HasValue ? 1 : 0),
            Metrics = assessment.Metrics.Select(m => new ProducerImpactAiMetricContext
            {
                MetricType = m.MetricType.ToString(),
                BeforeValue = m.BeforeValue,
                AfterValue = m.AfterValue,
                ChangeAbsolute = m.ChangeAbsolute,
                ChangePercentage = m.ChangePercentage,
                Status = m.Status.ToString(),
            }).ToList(),
            OverallSalesRank = afterReportDetails?.OverallSalesRank,
            OverallSalesPercentile = afterReportDetails?.OverallSalesPercentile,
            CategoryName = afterReportDetails?.Category?.Name,
            CategoryPosition = afterReportDetails?.CategoryPosition,
            CategoryAverageSales = afterReportDetails?.CategoryAverageSales,
            DistrictName = afterReportDetails?.District?.Name,
            DistrictPosition = afterReportDetails?.DistrictPosition,
            DistrictAverageSales = afterReportDetails?.DistrictAverageSales,
        };
    }

    private static ProducerImpactAIAnalysisDto ToAiDto(ProducerImpactAIAnalysis a) => new()
    {
        Id = a.Id,
        CaseId = a.CaseId,
        ImpactAssessmentId = a.ImpactAssessmentId,
        IsAiGenerated = a.IsAiGenerated,
        ProviderName = a.ProviderName,
        GeneratedAt = a.GeneratedAt,
        Findings = a.Findings.OrderBy(f => f.DisplayOrder).Select(f => new ProducerImpactFindingDto
        {
            Category = f.Category.ToString(),
            Kind = f.Kind.ToString(),
            Text = f.Text,
        }).ToList(),
    };

    /// <summary>
    /// Before = the calendar month immediately preceding the support. After = the calendar month the
    /// support fell in. Both are a deterministic function of supportProvidedAt, so recomputing later
    /// (e.g. once the After month's report exists) always targets the same two months. Pure calculation
    /// — no persistence, no AI/ML, every value comes from ProducerMonthlyReport rows already in the database.
    /// </summary>
    private async Task<(DateTime BeforeDate, DateTime AfterDate, ProducerMonthlyReport? BeforeReport, ProducerMonthlyReport? AfterReport, List<ArtisanSupportImpactMetric> Metrics)>
        ComputeAsync(Guid artisanUserId, DateTime supportProvidedAt, CancellationToken cancellationToken)
    {
        var afterDate = new DateTime(supportProvidedAt.Year, supportProvidedAt.Month, 1);
        var beforeDate = afterDate.AddMonths(-1);

        var beforeReport = await _reportRepository.GetAsync(artisanUserId, beforeDate.Year, beforeDate.Month, cancellationToken);
        var afterReport = await _reportRepository.GetAsync(artisanUserId, afterDate.Year, afterDate.Month, cancellationToken);

        var metrics = BuildMetrics(beforeReport, afterReport);

        return (beforeDate, afterDate, beforeReport, afterReport, metrics);
    }

    private List<ArtisanSupportImpactMetric> BuildMetrics(ProducerMonthlyReport? before, ProducerMonthlyReport? after) => new()
    {
        Build(ImpactMetricType.Sales, before?.TotalSales, after?.TotalSales, lowerIsBetter: false),
        Build(ImpactMetricType.Orders, before?.TotalOrders, after?.TotalOrders, lowerIsBetter: false),
        Build(ImpactMetricType.NetIncome, before?.NetIncome, after?.NetIncome, lowerIsBetter: false),
        Build(ImpactMetricType.UnitsSold, before?.UnitsSold, after?.UnitsSold, lowerIsBetter: false),
        Build(ImpactMetricType.AverageRating, before?.AverageRating, after?.AverageRating, lowerIsBetter: false),
        Build(ImpactMetricType.CancellationRate, before?.CancellationRate, after?.CancellationRate, lowerIsBetter: true),
        Build(ImpactMetricType.CustomerRetention, RetentionRate(before), RetentionRate(after), lowerIsBetter: false),
        Build(ImpactMetricType.ProductActivity, before?.ProductCount, after?.ProductCount, lowerIsBetter: false),
    };

    /// <summary>Returning customers as a percentage of all customers that month. Null when the month had no customers at all.</summary>
    private static decimal? RetentionRate(ProducerMonthlyReport? report)
    {
        if (report is null)
        {
            return null;
        }

        var totalCustomers = report.NewCustomers + report.ReturningCustomers;
        return totalCustomers == 0 ? null : Math.Round(report.ReturningCustomers * 100m / totalCustomers, 2);
    }

    /// <summary>
    /// Deterministic, threshold-based classification — no AI/ML. lowerIsBetter flips the sign so
    /// CancellationRate (where a decrease is the improvement) uses the same threshold comparison as
    /// every other metric.
    /// </summary>
    private ArtisanSupportImpactMetric Build(ImpactMetricType type, decimal? beforeValue, decimal? afterValue, bool lowerIsBetter)
    {
        var metric = new ArtisanSupportImpactMetric { MetricType = type, BeforeValue = beforeValue, AfterValue = afterValue };

        if (beforeValue is null || afterValue is null)
        {
            metric.Status = ImpactStatus.InsufficientData;
            return metric;
        }

        var changeAbsolute = afterValue.Value - beforeValue.Value;
        metric.ChangeAbsolute = changeAbsolute;

        if (beforeValue.Value == 0)
        {
            // No baseline to express a percentage against — classify from the raw direction instead.
            metric.Status = changeAbsolute == 0
                ? ImpactStatus.NoSignificantChange
                : (changeAbsolute > 0) != lowerIsBetter ? ImpactStatus.Improved : ImpactStatus.Declined;
            return metric;
        }

        var changePercentage = Math.Round(changeAbsolute / beforeValue.Value * 100m, 2);
        metric.ChangePercentage = changePercentage;

        var direction = lowerIsBetter ? -changePercentage : changePercentage;
        metric.Status = direction >= _thresholds.ImprovedThresholdPercentage ? ImpactStatus.Improved
            : direction <= -_thresholds.DeclinedThresholdPercentage ? ImpactStatus.Declined
            : ImpactStatus.NoSignificantChange;

        return metric;
    }

    private static ArtisanSupportImpactAssessmentDto ToDto(ArtisanSupportImpactAssessment a) => new()
    {
        Id = a.Id,
        CaseId = a.CaseId,
        BeforeYear = a.BeforeYear,
        BeforeMonth = a.BeforeMonth,
        BeforeReportAvailable = a.BeforeReportId.HasValue,
        AfterYear = a.AfterYear,
        AfterMonth = a.AfterMonth,
        AfterReportAvailable = a.AfterReportId.HasValue,
        GeneratedAt = a.GeneratedAt,
        Metrics = a.Metrics.Select(m => new ArtisanSupportImpactMetricDto
        {
            MetricType = m.MetricType.ToString(),
            BeforeValue = m.BeforeValue,
            AfterValue = m.AfterValue,
            ChangeAbsolute = m.ChangeAbsolute,
            ChangePercentage = m.ChangePercentage,
            Status = m.Status.ToString(),
        }).ToList(),
    };
}
