using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

internal static class FieldResearchMappings
{
    public static SurveyQuestionDto ToDto(this SurveyQuestion q) => new()
    {
        Id = q.Id,
        SurveyId = q.SurveyId,
        Text = q.Text,
        HelpText = q.HelpText,
        QuestionType = q.QuestionType.ToString(),
        IsRequired = q.IsRequired,
        OrderIndex = q.OrderIndex,
        OptionsJson = q.OptionsJson,
        MinValue = q.MinValue,
        MaxValue = q.MaxValue,
    };

    public static SurveyFieldAssignmentDto ToDto(this SurveyFieldAssignment a) => new()
    {
        Id = a.Id,
        SurveyId = a.SurveyId,
        FieldResearcherUserId = a.FieldResearcherUserId,
        FieldResearcherName = a.FieldResearcher?.FullName ?? string.Empty,
        FieldResearcherEmail = a.FieldResearcher?.Email ?? string.Empty,
        Role = a.Role.ToString(),
        AreaNote = a.AreaNote,
        IsActive = a.IsActive,
        AssignedByUserId = a.AssignedByUserId,
        AssignedAt = a.AssignedAt,
    };

    public static DataCollectionEventDto ToDto(this DataCollectionEvent e) => new()
    {
        Id = e.Id,
        ActorUserId = e.ActorUserId,
        ActorName = e.Actor?.FullName ?? string.Empty,
        EventType = e.EventType.ToString(),
        Summary = e.Summary,
        SurveyResponseId = e.SurveyResponseId,
        CreatedAt = e.CreatedAt,
    };

    public static FieldEvidenceDto ToDto(this FieldEvidence e) => new()
    {
        Id = e.Id,
        SurveyId = e.SurveyId,
        SurveyResponseId = e.SurveyResponseId,
        CapturedByUserId = e.CapturedByUserId,
        CapturedByName = e.CapturedBy?.FullName ?? string.Empty,
        EvidenceType = e.EvidenceType.ToString(),
        Title = e.Title,
        Description = e.Description,
        FileUrl = e.FileUrl,
        FileName = e.FileName,
        MimeType = e.MimeType,
        FileSizeBytes = e.FileSizeBytes,
        DurationSeconds = e.DurationSeconds,
        TranscriptText = e.TranscriptText,
        Language = e.Language,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        LocationAccuracyMeters = e.LocationAccuracyMeters,
        CapturedAt = e.CapturedAt,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static SurveyResponseAnswerDto ToDto(this SurveyResponseAnswer a) => new()
    {
        Id = a.Id,
        SurveyQuestionId = a.SurveyQuestionId,
        QuestionText = a.Question?.Text ?? string.Empty,
        QuestionType = a.Question?.QuestionType.ToString() ?? string.Empty,
        ValueText = a.ValueText,
        ValueNumber = a.ValueNumber,
        ValueBoolean = a.ValueBoolean,
        ValueDate = a.ValueDate,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
    };

    public static SurveyResponseListItemDto ToListItemDto(this SurveyResponse r) => new()
    {
        Id = r.Id,
        SurveyId = r.SurveyId,
        Status = r.Status.ToString(),
        Source = r.Source.ToString(),
        RespondentName = r.RespondentName,
        VillageName = r.VillageName,
        DistrictName = r.DistrictName,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        SubmittedByUserId = r.SubmittedByUserId,
        SubmittedByName = r.SubmittedBy?.FullName,
        AnswerCount = r.Answers.Count,
        EvidenceCount = r.Evidence.Count,
        CollectedAt = r.CollectedAt,
        SubmittedAt = r.SubmittedAt,
        UpdatedAt = r.UpdatedAt,
    };

    public static SurveyResponseDto ToDto(this SurveyResponse r) => new()
    {
        Id = r.Id,
        SurveyId = r.SurveyId,
        Status = r.Status.ToString(),
        Source = r.Source.ToString(),
        RespondentName = r.RespondentName,
        RespondentContact = r.RespondentContact,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        LocationAccuracyMeters = r.LocationAccuracyMeters,
        VillageName = r.VillageName,
        DistrictName = r.DistrictName,
        SubmittedByUserId = r.SubmittedByUserId,
        SubmittedByName = r.SubmittedBy?.FullName,
        ReviewNote = r.ReviewNote,
        ReviewedByUserId = r.ReviewedByUserId,
        ReviewedByName = r.ReviewedBy?.FullName,
        ReviewedAt = r.ReviewedAt,
        CollectedAt = r.CollectedAt,
        SubmittedAt = r.SubmittedAt,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Answers = r.Answers.OrderBy(a => a.Question != null ? a.Question.OrderIndex : 0).Select(a => a.ToDto()).ToList(),
        Evidence = r.Evidence.OrderByDescending(e => e.CapturedAt).Select(e => e.ToDto()).ToList(),
    };

    public static SurveyListItemDto ToListItemDto(this Survey s, Guid userId)
    {
        var assignment = s.FieldAssignments.FirstOrDefault(a => a.FieldResearcherUserId == userId);
        var myRole = s.OwnerUserId == userId
            ? "Owner"
            : assignment?.Role.ToString() ?? string.Empty;

        return new SurveyListItemDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = s.Title,
            Status = s.Status.ToString(),
            TargetRegion = s.TargetRegion,
            Language = s.Language,
            OwnerUserId = s.OwnerUserId,
            OwnerName = s.Owner?.FullName ?? string.Empty,
            ResearchProjectId = s.ResearchProjectId,
            MyRole = myRole,
            QuestionCount = s.Questions.Count,
            FieldResearcherCount = s.FieldAssignments.Count(a => a.IsActive),
            ResponseCount = s.ResponseCount,
            OpensAt = s.OpensAt,
            ClosesAt = s.ClosesAt,
            UpdatedAt = s.UpdatedAt,
        };
    }
}
