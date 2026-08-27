using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

internal static class ResearchMappings
{
    public static ResearchProjectMemberDto ToDto(this ResearchProjectMember member) => new()
    {
        Id = member.Id,
        UserId = member.UserId,
        UserName = member.User?.FullName ?? string.Empty,
        UserEmail = member.User?.Email ?? string.Empty,
        Role = member.Role.ToString(),
        InvitedByUserId = member.InvitedByUserId,
        JoinedAt = member.JoinedAt,
    };

    public static ResearchActivityDto ToDto(this ResearchActivity activity) => new()
    {
        Id = activity.Id,
        ActorUserId = activity.ActorUserId,
        ActorName = activity.Actor?.FullName ?? string.Empty,
        Type = activity.Type.ToString(),
        Summary = activity.Summary,
        CreatedAt = activity.CreatedAt,
    };

    public static ResearchTaskDto ToDto(this ResearchTask task) => new()
    {
        Id = task.Id,
        ResearchProjectId = task.ResearchProjectId,
        MilestoneId = task.MilestoneId,
        MilestoneTitle = task.Milestone?.Title,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status.ToString(),
        Priority = task.Priority.ToString(),
        AssignedToUserId = task.AssignedToUserId,
        AssignedToName = task.AssignedTo?.FullName,
        CreatedByUserId = task.CreatedByUserId,
        CreatedByName = task.CreatedBy?.FullName ?? string.Empty,
        DueDate = task.DueDate,
        CompletedAt = task.CompletedAt,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
    };

    public static ResearchMilestoneDto ToDto(this ResearchMilestone milestone, int taskCount) => new()
    {
        Id = milestone.Id,
        ResearchProjectId = milestone.ResearchProjectId,
        Title = milestone.Title,
        Description = milestone.Description,
        Status = milestone.Status.ToString(),
        TargetDate = milestone.TargetDate,
        AchievedAt = milestone.AchievedAt,
        OrderIndex = milestone.OrderIndex,
        TaskCount = taskCount,
        CreatedAt = milestone.CreatedAt,
        UpdatedAt = milestone.UpdatedAt,
    };

    public static ResearchNoteDto ToDto(this ResearchNote note) => new()
    {
        Id = note.Id,
        ResearchProjectId = note.ResearchProjectId,
        AuthorUserId = note.AuthorUserId,
        AuthorName = note.Author?.FullName ?? string.Empty,
        Title = note.Title,
        Content = note.Content,
        Visibility = note.Visibility.ToString(),
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
    };

    public static ResearchPaperDto ToDto(this ResearchPaper paper) => new()
    {
        Id = paper.Id,
        ResearchProjectId = paper.ResearchProjectId,
        Title = paper.Title,
        Abstract = paper.Abstract,
        Authors = paper.Authors,
        Keywords = paper.Keywords,
        Status = paper.Status.ToString(),
        ManuscriptUrl = paper.ManuscriptUrl,
        TargetVenue = paper.TargetVenue,
        CreatedByUserId = paper.CreatedByUserId,
        CreatedByName = paper.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = paper.CreatedAt,
        UpdatedAt = paper.UpdatedAt,
    };

    public static ResearchAIFindingDto ToDto(this ResearchAIFinding f) => new()
    {
        Id = f.Id,
        Category = f.Category.ToString(),
        Heading = f.Heading,
        Detail = f.Detail,
        Metric = f.Metric,
        Score = f.Score,
        DisplayOrder = f.DisplayOrder,
    };

    public static ResearchAICitationDto ToDto(this ResearchAICitation c) => new()
    {
        Id = c.Id,
        ResearchPublicationId = c.ResearchPublicationId,
        Style = c.Style.ToString(),
        SourceTitle = c.SourceTitle,
        Authors = c.Authors,
        Year = c.Year,
        Container = c.Container,
        Doi = c.Doi,
        Url = c.Url,
        FormattedCitation = c.FormattedCitation,
        DisplayOrder = c.DisplayOrder,
    };

    public static ResearchAIAnalysisListItemDto ToListItemDto(this ResearchAIAnalysis a) => new()
    {
        Id = a.Id,
        ResearchProjectId = a.ResearchProjectId,
        AnalysisType = a.AnalysisType.ToString(),
        Status = a.Status.ToString(),
        ProviderName = a.ProviderName,
        Title = a.Title,
        ResultSummary = a.ResultSummary,
        Confidence = a.Confidence,
        FindingCount = a.Findings.Count,
        CitationCount = a.Citations.Count,
        RequestedByUserId = a.RequestedByUserId,
        RequestedByName = a.RequestedBy?.FullName ?? string.Empty,
        CreatedAt = a.CreatedAt,
        CompletedAt = a.CompletedAt,
    };

    public static ResearchAIAnalysisDto ToDto(this ResearchAIAnalysis a) => new()
    {
        Id = a.Id,
        ResearchProjectId = a.ResearchProjectId,
        AnalysisType = a.AnalysisType.ToString(),
        Status = a.Status.ToString(),
        ProviderName = a.ProviderName,
        Title = a.Title,
        ResearchQuestions = a.ResearchQuestions,
        InputSummary = a.InputSummary,
        ResultSummary = a.ResultSummary,
        ResultJson = a.ResultJson,
        ContextJson = a.ContextJson,
        Confidence = a.Confidence,
        ErrorMessage = a.ErrorMessage,
        DatasetId = a.DatasetId,
        DatasetName = a.Dataset?.Name,
        ResearchPaperId = a.ResearchPaperId,
        PaperTitle = a.Paper?.Title,
        RequestedByUserId = a.RequestedByUserId,
        RequestedByName = a.RequestedBy?.FullName ?? string.Empty,
        CreatedAt = a.CreatedAt,
        CompletedAt = a.CompletedAt,
        Findings = a.Findings.OrderBy(f => f.DisplayOrder).Select(f => f.ToDto()).ToList(),
        Citations = a.Citations.OrderBy(c => c.DisplayOrder).Select(c => c.ToDto()).ToList(),
    };

    public static ResearchPublicationDto ToDto(this ResearchPublication publication) => new()
    {
        Id = publication.Id,
        ResearchProjectId = publication.ResearchProjectId,
        ProjectTitle = publication.Project?.Title ?? string.Empty,
        ResearchPaperId = publication.ResearchPaperId,
        Title = publication.Title,
        Authors = publication.Authors,
        Venue = publication.Venue,
        Type = publication.Type.ToString(),
        Doi = publication.Doi,
        Url = publication.Url,
        Abstract = publication.Abstract,
        Citation = publication.Citation,
        PublishedOn = publication.PublishedOn,
        IsPublic = publication.IsPublic,
        CreatedByUserId = publication.CreatedByUserId,
        CreatedByName = publication.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = publication.CreatedAt,
        UpdatedAt = publication.UpdatedAt,
    };
}
