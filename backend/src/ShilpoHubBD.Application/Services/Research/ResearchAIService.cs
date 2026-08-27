using System.Text;
using System.Text.Json;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchAIService : IResearchAIService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly IResearchAIProvider _provider;
    private readonly IResearchProjectRepository _projectRepository;
    private readonly IHeritageDatasetRepository _datasetRepository;
    private readonly IResearchAIAnalysisRepository _analysisRepository;

    public ResearchAIService(
        IResearchAIProvider provider,
        IResearchProjectRepository projectRepository,
        IHeritageDatasetRepository datasetRepository,
        IResearchAIAnalysisRepository analysisRepository)
    {
        _provider = provider;
        _projectRepository = projectRepository;
        _datasetRepository = datasetRepository;
        _analysisRepository = analysisRepository;
    }

    public Task<ResearchAIAnalysisDto> RunInsightsAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => RunAnalysisAsync(userId, projectId, request, ResearchAIAnalysisType.AutomaticInsights,
            "Automatic insights", _provider.GenerateInsightsAsync, cancellationToken);

    public Task<ResearchAIAnalysisDto> RunTrendDiscoveryAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => RunAnalysisAsync(userId, projectId, request, ResearchAIAnalysisType.TrendDiscovery,
            "Trend discovery", _provider.DiscoverTrendsAsync, cancellationToken);

    public Task<ResearchAIAnalysisDto> RunCorrelationDetectionAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => RunAnalysisAsync(userId, projectId, request, ResearchAIAnalysisType.CorrelationDetection,
            "Correlation detection", _provider.DetectCorrelationsAsync, cancellationToken);

    public Task<ResearchAIAnalysisDto> RunReportGenerationAsync(
        Guid userId, Guid projectId, RunResearchAnalysisRequest request, CancellationToken cancellationToken)
        => RunAnalysisAsync(userId, projectId, request, ResearchAIAnalysisType.ReportGeneration,
            "Report generation", _provider.GenerateReportAsync, cancellationToken);

    private async Task<ResearchAIAnalysisDto> RunAnalysisAsync(
        Guid userId,
        Guid projectId,
        RunResearchAnalysisRequest request,
        ResearchAIAnalysisType type,
        string defaultTitle,
        Func<ResearchAnalysisContext, CancellationToken, Task<ResearchAnalysisResult>> invoke,
        CancellationToken cancellationToken)
    {
        var project = await LoadProjectForRunAsync(userId, projectId, cancellationToken);

        var (dataset, datasetId) = await ResolveDatasetAsync(userId, request.DatasetId, cancellationToken);
        var paper = await ResolvePaperAsync(projectId, request.ResearchPaperId, cancellationToken);

        var context = BuildContext(project, request, dataset, paper);
        var result = await invoke(context, cancellationToken);

        var now = DateTime.UtcNow;
        var analysis = new ResearchAIAnalysis
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            RequestedByUserId = userId,
            AnalysisType = type,
            Status = ResearchAIAnalysisStatus.Completed,
            ProviderName = result.ProviderName,
            Title = string.IsNullOrWhiteSpace(request.Title) ? defaultTitle : request.Title!.Trim(),
            ResearchQuestions = JoinQuestions(request.ResearchQuestions),
            InputSummary = BuildInputSummary(request, dataset, paper),
            ContextJson = Serialize(context),
            ResultSummary = result.Summary,
            ResultJson = Serialize(result.Items),
            Confidence = result.Confidence,
            DatasetId = datasetId,
            ResearchPaperId = paper?.Id,
            CreatedAt = now,
            CompletedAt = now,
        };

        var order = 0;
        foreach (var item in result.Items)
        {
            analysis.Findings.Add(new ResearchAIFinding
            {
                Id = Guid.NewGuid(),
                Category = item.Category,
                Heading = Trim(item.Heading, 300),
                Detail = Trim(item.Detail, 4000),
                Metric = string.IsNullOrWhiteSpace(item.Metric) ? null : Trim(item.Metric!, 120),
                Score = item.Score,
                DisplayOrder = order++,
            });
        }

        await _analysisRepository.AddAsync(analysis, cancellationToken);
        await _analysisRepository.SaveChangesAsync(cancellationToken);

        return (await _analysisRepository.GetByIdAsync(analysis.Id, cancellationToken))!.ToDto();
    }

    public async Task<ResearchAIAnalysisDto> GenerateCitationsAsync(
        Guid userId, Guid projectId, GenerateResearchCitationsRequest request, CancellationToken cancellationToken)
    {
        var project = await LoadProjectForRunAsync(userId, projectId, cancellationToken);

        if (!Enum.TryParse<ResearchCitationStyle>(request.Style, true, out var style))
        {
            throw new ConflictException("Style must be one of: Apa, Mla, Chicago, Ieee, Bibtex.");
        }

        var sources = new List<ResearchCitationSourceDto>(request.Sources);
        foreach (var publicationId in request.PublicationIds.Distinct())
        {
            var publication = await _projectRepository.GetPublicationByIdAsync(publicationId, cancellationToken);
            if (publication is null || publication.ResearchProjectId != projectId)
            {
                throw new NotFoundException("Publication not found in this project.");
            }

            sources.Add(new ResearchCitationSourceDto
            {
                Title = publication.Title,
                Authors = publication.Authors,
                Year = publication.PublishedOn?.Year,
                Container = publication.Venue,
                Doi = publication.Doi,
                Url = publication.Url,
                ResearchPublicationId = publication.Id,
            });
        }

        if (sources.Count == 0)
        {
            throw new ConflictException("Provide at least one citation source or publication id.");
        }

        var context = new ResearchCitationContext
        {
            ProjectTitle = project.Title,
            Style = style,
            Sources = sources,
        };

        var result = await _provider.GenerateCitationsAsync(context, cancellationToken);

        var now = DateTime.UtcNow;
        var analysis = new ResearchAIAnalysis
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            RequestedByUserId = userId,
            AnalysisType = ResearchAIAnalysisType.CitationGeneration,
            Status = ResearchAIAnalysisStatus.Completed,
            ProviderName = result.ProviderName,
            Title = string.IsNullOrWhiteSpace(request.Title) ? $"Citations ({style})" : request.Title!.Trim(),
            ResearchQuestions = string.Empty,
            InputSummary = $"{sources.Count} source(s) formatted in {style} style.",
            ContextJson = Serialize(context),
            ResultSummary = result.Summary,
            ResultJson = Serialize(result.Items),
            CreatedAt = now,
            CompletedAt = now,
        };

        var order = 0;
        foreach (var item in result.Items)
        {
            analysis.Citations.Add(new ResearchAICitation
            {
                Id = Guid.NewGuid(),
                ResearchPublicationId = item.ResearchPublicationId,
                Style = item.Style,
                SourceTitle = Trim(item.SourceTitle, 400),
                Authors = string.IsNullOrWhiteSpace(item.Authors) ? null : Trim(item.Authors!, 2000),
                Year = item.Year,
                Container = string.IsNullOrWhiteSpace(item.Container) ? null : Trim(item.Container!, 400),
                Doi = string.IsNullOrWhiteSpace(item.Doi) ? null : Trim(item.Doi!, 200),
                Url = string.IsNullOrWhiteSpace(item.Url) ? null : Trim(item.Url!, 2048),
                FormattedCitation = Trim(item.FormattedCitation, 4000),
                DisplayOrder = order++,
            });
        }

        await _analysisRepository.AddAsync(analysis, cancellationToken);
        await _analysisRepository.SaveChangesAsync(cancellationToken);

        return (await _analysisRepository.GetByIdAsync(analysis.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<ResearchAIAnalysisListItemDto>> GetForProjectAsync(
        Guid userId, Guid projectId, ResearchAIAnalysisQueryParameters query, CancellationToken cancellationToken)
    {
        await LoadProjectForReadAsync(userId, projectId, cancellationToken);

        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _analysisRepository.GetPagedForProjectAsync(projectId, query, cancellationToken);
        return new PagedResult<ResearchAIAnalysisListItemDto>
        {
            Items = items.Select(a => a.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ResearchAIAnalysisDto> GetByIdAsync(
        Guid userId, Guid projectId, Guid analysisId, CancellationToken cancellationToken)
    {
        await LoadProjectForReadAsync(userId, projectId, cancellationToken);
        var analysis = await LoadAnalysisAsync(projectId, analysisId, cancellationToken);
        return analysis.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid analysisId, CancellationToken cancellationToken)
    {
        var (_, membership) = await LoadProjectAsync(userId, projectId, cancellationToken);
        var member = ResearchAccess.RequireRole(membership, ResearchRole.Researcher);

        var analysis = await LoadAnalysisAsync(projectId, analysisId, cancellationToken);
        if (analysis.RequestedByUserId != userId && !member.Role.AtLeast(ResearchRole.Admin))
        {
            throw new UnauthorizedAccessException("Only the requester or a project admin can delete this analysis.");
        }

        _analysisRepository.Remove(analysis);
        await _analysisRepository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ------------------------------------------------------

    private async Task<(ResearchProject Project, ResearchProjectMember? Membership)> LoadProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");
        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        return (project, membership);
    }

    private async Task<ResearchProject> LoadProjectForRunAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var (project, membership) = await LoadProjectAsync(userId, projectId, cancellationToken);
        ResearchAccess.RequireRole(membership, ResearchRole.Researcher);
        return project;
    }

    private async Task LoadProjectForReadAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var (project, membership) = await LoadProjectAsync(userId, projectId, cancellationToken);
        ResearchAccess.RequireReadAccess(project, membership);
    }

    private async Task<ResearchAIAnalysis> LoadAnalysisAsync(Guid projectId, Guid analysisId, CancellationToken cancellationToken)
    {
        var analysis = await _analysisRepository.GetByIdAsync(analysisId, cancellationToken);
        if (analysis is null || analysis.ResearchProjectId != projectId)
        {
            throw new NotFoundException("AI analysis not found.");
        }

        return analysis;
    }

    private async Task<(HeritageDataset? Dataset, Guid? DatasetId)> ResolveDatasetAsync(
        Guid userId, Guid? datasetId, CancellationToken cancellationToken)
    {
        if (!datasetId.HasValue)
        {
            return (null, null);
        }

        var dataset = await _datasetRepository.GetByIdAsync(datasetId.Value, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var hasGrant = dataset.AccessGrants.Any(g =>
            g.UserId == userId && (g.ExpiresAt is null || g.ExpiresAt > DateTime.UtcNow));

        if (dataset.AccessLevel == HeritageDatasetAccessLevel.Restricted
            && dataset.OwnerUserId != userId
            && !hasGrant)
        {
            throw new ConflictException("You do not have access to the linked dataset.");
        }

        return (dataset, dataset.Id);
    }

    private async Task<ResearchPaper?> ResolvePaperAsync(Guid projectId, Guid? paperId, CancellationToken cancellationToken)
    {
        if (!paperId.HasValue)
        {
            return null;
        }

        var paper = await _projectRepository.GetPaperByIdAsync(paperId.Value, cancellationToken);
        if (paper is null || paper.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Research paper not found in this project.");
        }

        return paper;
    }

    private static ResearchAnalysisContext BuildContext(
        ResearchProject project, RunResearchAnalysisRequest request, HeritageDataset? dataset, ResearchPaper? paper)
        => new()
        {
            ProjectId = project.Id,
            ProjectTitle = project.Title,
            Discipline = project.Discipline,
            ResearchQuestions = request.ResearchQuestions
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .Select(q => q.Trim())
                .ToList(),
            Notes = request.Notes?.Trim(),
            DatasetName = dataset?.Name,
            DatasetCategory = dataset?.Category.ToString(),
            DatasetRecordCount = dataset?.RecordCount,
            DatasetTags = dataset?.Tags,
            PaperTitle = paper?.Title,
            PaperAbstract = paper?.Abstract,
            PaperKeywords = paper?.Keywords,
            RangeStart = request.RangeStart,
            RangeEnd = request.RangeEnd,
            SelectedData = request.SelectedData ?? new List<ResearchDataPointDto>(),
        };

    private static string BuildInputSummary(RunResearchAnalysisRequest request, HeritageDataset? dataset, ResearchPaper? paper)
    {
        var parts = new List<string>
        {
            $"{request.ResearchQuestions.Count(q => !string.IsNullOrWhiteSpace(q))} research question(s)",
            $"{request.SelectedData?.Count ?? 0} selected data point(s)",
        };

        if (dataset is not null)
        {
            parts.Add($"dataset \"{dataset.Name}\" ({dataset.Category})");
        }

        if (paper is not null)
        {
            parts.Add($"paper \"{paper.Title}\"");
        }

        if (request.RangeStart.HasValue || request.RangeEnd.HasValue)
        {
            parts.Add($"range {request.RangeStart:yyyy-MM-dd}..{request.RangeEnd:yyyy-MM-dd}");
        }

        return string.Join("; ", parts);
    }

    private static string JoinQuestions(IEnumerable<string> questions)
    {
        var joined = string.Join("\n", questions.Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => q.Trim()));
        return joined.Length > 4000 ? joined[..4000] : joined;
    }

    private static string Serialize<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return json.Length > 32000 ? json[..32000] : json;
    }

    private static string Trim(string value, int max)
        => string.IsNullOrEmpty(value) ? string.Empty : value.Length > max ? value[..max] : value;
}
