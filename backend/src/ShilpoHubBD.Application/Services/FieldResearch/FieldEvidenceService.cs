using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

public class FieldEvidenceService : SurveyServiceBase, IFieldEvidenceService
{
    public FieldEvidenceService(ISurveyRepository repository, IResearchProjectRepository projectRepository)
        : base(repository, projectRepository)
    {
    }

    public async Task<PagedResult<FieldEvidenceDto>> GetForSurveyAsync(
        Guid userId, Guid surveyId, FieldEvidenceQueryParameters query, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await Repository.GetEvidenceAsync(surveyId, query, cancellationToken);
        var filtered = query.MineOnly
            ? items.Where(e => e.CapturedByUserId == userId).ToList()
            : items;

        return new PagedResult<FieldEvidenceDto>
        {
            Items = filtered.Select(e => e.ToDto()).ToList(),
            TotalCount = query.MineOnly ? filtered.Count : totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<FieldEvidenceDto> GetByIdAsync(
        Guid userId, Guid surveyId, Guid evidenceId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        var evidence = await LoadEvidenceAsync(surveyId, evidenceId, cancellationToken);
        return evidence.ToDto();
    }

    public async Task<FieldEvidenceDto> CreateAsync(
        Guid userId, Guid surveyId, CreateFieldEvidenceRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireContributor(survey, userId);

        var type = ParseType(request.EvidenceType);
        await ValidateResponseLinkAsync(surveyId, request.SurveyResponseId, cancellationToken);

        var now = DateTime.UtcNow;
        var evidence = new FieldEvidence
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            SurveyResponseId = request.SurveyResponseId,
            CapturedByUserId = userId,
            EvidenceType = type,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            FileUrl = request.FileUrl?.Trim(),
            FileName = request.FileName?.Trim(),
            MimeType = request.MimeType?.Trim(),
            FileSizeBytes = request.FileSizeBytes,
            DurationSeconds = request.DurationSeconds,
            TranscriptText = request.TranscriptText?.Trim(),
            Language = request.Language?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationAccuracyMeters = request.LocationAccuracyMeters,
            CapturedAt = request.CapturedAt ?? now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddEvidenceAsync(evidence, cancellationToken);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.EvidenceAdded,
            $"{type} evidence added: \"{Truncate(evidence.Title, 80)}\".", request.SurveyResponseId, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadEvidenceAsync(surveyId, evidence.Id, cancellationToken)).ToDto();
    }

    public async Task<FieldEvidenceDto> UpdateAsync(
        Guid userId, Guid surveyId, Guid evidenceId, UpdateFieldEvidenceRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        var evidence = await LoadEvidenceAsync(surveyId, evidenceId, cancellationToken);
        EnsureCanMutate(survey, evidence, userId);

        var type = ParseType(request.EvidenceType);
        await ValidateResponseLinkAsync(surveyId, request.SurveyResponseId, cancellationToken);

        evidence.SurveyResponseId = request.SurveyResponseId;
        evidence.EvidenceType = type;
        evidence.Title = request.Title.Trim();
        evidence.Description = request.Description?.Trim();
        evidence.FileUrl = request.FileUrl?.Trim();
        evidence.FileName = request.FileName?.Trim();
        evidence.MimeType = request.MimeType?.Trim();
        evidence.FileSizeBytes = request.FileSizeBytes;
        evidence.DurationSeconds = request.DurationSeconds;
        evidence.TranscriptText = request.TranscriptText?.Trim();
        evidence.Language = request.Language?.Trim();
        evidence.Latitude = request.Latitude;
        evidence.Longitude = request.Longitude;
        evidence.LocationAccuracyMeters = request.LocationAccuracyMeters;
        if (request.CapturedAt.HasValue)
        {
            evidence.CapturedAt = request.CapturedAt.Value;
        }

        evidence.UpdatedAt = DateTime.UtcNow;

        await Repository.SaveChangesAsync(cancellationToken);
        return (await LoadEvidenceAsync(surveyId, evidence.Id, cancellationToken)).ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid surveyId, Guid evidenceId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        var evidence = await LoadEvidenceAsync(surveyId, evidenceId, cancellationToken);
        EnsureCanMutate(survey, evidence, userId);

        Repository.RemoveEvidence(evidence);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.EvidenceRemoved,
            $"Evidence removed: \"{Truncate(evidence.Title, 80)}\".", evidence.SurveyResponseId, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----------------------------------------------------

    private async Task<FieldEvidence> LoadEvidenceAsync(Guid surveyId, Guid evidenceId, CancellationToken cancellationToken)
    {
        var evidence = await Repository.GetEvidenceByIdAsync(evidenceId, cancellationToken);
        if (evidence is null || evidence.SurveyId != surveyId)
        {
            throw new NotFoundException("Field evidence not found.");
        }

        return evidence;
    }

    private async Task ValidateResponseLinkAsync(Guid surveyId, Guid? responseId, CancellationToken cancellationToken)
    {
        if (!responseId.HasValue)
        {
            return;
        }

        var response = await Repository.GetResponseByIdAsync(responseId.Value, cancellationToken);
        if (response is null || response.SurveyId != surveyId)
        {
            throw new NotFoundException("The linked survey response was not found in this survey.");
        }
    }

    private static void EnsureCanMutate(Survey survey, FieldEvidence evidence, Guid userId)
    {
        if (FieldResearchAccess.IsOwner(survey, userId) || evidence.CapturedByUserId == userId)
        {
            return;
        }

        var assignment = FieldResearchAccess.ActiveAssignment(survey, userId);
        if (assignment is not null && assignment.Role == FieldAssignmentRole.Supervisor)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "Only the person who captured this evidence, the survey owner, or a Supervisor can modify it.");
    }

    private static FieldEvidenceType ParseType(string value)
        => Enum.TryParse<FieldEvidenceType>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException(
                "EvidenceType must be one of: Photo, AudioRecording, VideoRecording, InterviewTranscript, Document, GpsWaypoint, Note.");

    private static string Truncate(string value, int max)
        => value.Length > max ? value[..max] : value;
}
