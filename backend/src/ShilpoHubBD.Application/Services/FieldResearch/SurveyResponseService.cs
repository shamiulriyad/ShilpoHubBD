using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

public class SurveyResponseService : SurveyServiceBase, ISurveyResponseService
{
    public SurveyResponseService(ISurveyRepository repository, IResearchProjectRepository projectRepository)
        : base(repository, projectRepository)
    {
    }

    public async Task<PagedResult<SurveyResponseListItemDto>> GetForSurveyAsync(
        Guid userId, Guid surveyId, SurveyResponseQueryParameters query, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        // Collectors only ever see their own responses; owner / supervisor / reviewer see all.
        if (!CanSeeAllResponses(survey, userId) || query.MineOnly)
        {
            query.SubmittedByUserId = userId;
        }

        var (items, totalCount) = await Repository.GetResponsesAsync(surveyId, query, cancellationToken);
        return new PagedResult<SurveyResponseListItemDto>
        {
            Items = items.Select(r => r.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<SurveyResponseDto> GetByIdAsync(
        Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        var response = await LoadResponseAsync(surveyId, responseId, cancellationToken);
        if (!CanSeeAllResponses(survey, userId) && response.SubmittedByUserId != userId)
        {
            throw new NotFoundException("Survey response not found.");
        }

        return response.ToDto();
    }

    public async Task<SurveyResponseDto> CreateAsync(
        Guid userId, Guid surveyId, CreateSurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireContributor(survey, userId);

        if (survey.Status != SurveyStatus.Active)
        {
            throw new ConflictException("This survey is not currently accepting responses.");
        }

        var source = ParseSource(request.Source) ?? FieldResponseSource.FieldInterview;
        var questions = survey.Questions.ToList();
        var answers = BuildAnswers(request.Answers, questions);

        var now = DateTime.UtcNow;
        var response = new SurveyResponse
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            SubmittedByUserId = userId,
            RespondentName = request.RespondentName?.Trim(),
            RespondentContact = request.RespondentContact?.Trim(),
            Status = SurveyResponseStatus.Draft,
            Source = source,
            CollectedAt = request.CollectedAt ?? now,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationAccuracyMeters = request.LocationAccuracyMeters,
            VillageName = request.VillageName?.Trim(),
            DistrictName = request.DistrictName?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var answer in answers)
        {
            answer.SurveyResponseId = response.Id;
            response.Answers.Add(answer);
        }

        if (request.SubmitNow)
        {
            EnsureRequiredAnswered(questions, response.Answers);
            response.Status = SurveyResponseStatus.Submitted;
            response.SubmittedAt = now;
            survey.ResponseCount += 1;
        }

        await Repository.AddResponseAsync(response, cancellationToken);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.ResponseCreated,
            $"Response collected{(request.SubmitNow ? " and submitted" : "")}.", response.Id, cancellationToken);
        if (request.SubmitNow)
        {
            await AddEventAsync(surveyId, userId, DataCollectionEventType.ResponseSubmitted,
                "Response submitted.", response.Id, cancellationToken);
        }

        await Repository.SaveChangesAsync(cancellationToken);
        return (await LoadResponseAsync(surveyId, response.Id, cancellationToken)).ToDto();
    }

    public async Task<SurveyResponseDto> UpdateAsync(
        Guid userId, Guid surveyId, Guid responseId, UpdateSurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireContributor(survey, userId);

        var response = await LoadResponseAsync(surveyId, responseId, cancellationToken);
        EnsureCanEditResponse(survey, response, userId);

        if (response.Status is not (SurveyResponseStatus.Draft or SurveyResponseStatus.Rejected))
        {
            throw new ConflictException("Only draft or rejected responses can be edited.");
        }

        var source = ParseSource(request.Source)
            ?? throw new ConflictException("Source must be one of: FieldInterview, PhoneInterview, SelfReported, Import.");

        var questions = survey.Questions.ToList();
        var newAnswers = BuildAnswers(request.Answers, questions);

        response.Source = source;
        response.RespondentName = request.RespondentName?.Trim();
        response.RespondentContact = request.RespondentContact?.Trim();
        response.Latitude = request.Latitude;
        response.Longitude = request.Longitude;
        response.LocationAccuracyMeters = request.LocationAccuracyMeters;
        response.VillageName = request.VillageName?.Trim();
        response.DistrictName = request.DistrictName?.Trim();
        if (request.CollectedAt.HasValue)
        {
            response.CollectedAt = request.CollectedAt.Value;
        }

        response.UpdatedAt = DateTime.UtcNow;

        Repository.RemoveAnswers(response.Answers.ToList());
        response.Answers.Clear();
        foreach (var answer in newAnswers)
        {
            answer.SurveyResponseId = response.Id;
            response.Answers.Add(answer);
        }

        await Repository.SaveChangesAsync(cancellationToken);
        return (await LoadResponseAsync(surveyId, response.Id, cancellationToken)).ToDto();
    }

    public async Task<SurveyResponseDto> SubmitAsync(
        Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireContributor(survey, userId);

        var response = await LoadResponseAsync(surveyId, responseId, cancellationToken);
        EnsureCanEditResponse(survey, response, userId);

        if (response.Status is not (SurveyResponseStatus.Draft or SurveyResponseStatus.Rejected))
        {
            throw new ConflictException("Only draft or rejected responses can be submitted.");
        }

        EnsureRequiredAnswered(survey.Questions.ToList(), response.Answers);

        var wasCounted = response.Status != SurveyResponseStatus.Draft;
        response.Status = SurveyResponseStatus.Submitted;
        response.SubmittedAt = DateTime.UtcNow;
        response.SubmittedByUserId ??= userId;
        response.UpdatedAt = DateTime.UtcNow;

        if (!wasCounted)
        {
            survey.ResponseCount += 1;
        }

        await AddEventAsync(surveyId, userId, DataCollectionEventType.ResponseSubmitted,
            "Response submitted.", response.Id, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadResponseAsync(surveyId, response.Id, cancellationToken)).ToDto();
    }

    public async Task<SurveyResponseDto> ReviewAsync(
        Guid userId, Guid surveyId, Guid responseId, ReviewSurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireReviewer(survey, userId);

        var response = await LoadResponseAsync(surveyId, responseId, cancellationToken);
        if (response.Status is not (SurveyResponseStatus.Submitted or SurveyResponseStatus.UnderReview))
        {
            throw new ConflictException("Only submitted or under-review responses can be reviewed.");
        }

        var status = request.Decision?.Trim().ToLowerInvariant() switch
        {
            "approve" or "approved" => SurveyResponseStatus.Approved,
            "reject" or "rejected" => SurveyResponseStatus.Rejected,
            "review" or "underreview" or "under_review" => SurveyResponseStatus.UnderReview,
            _ => throw new ConflictException("Decision must be one of: approve, reject, review."),
        };

        response.Status = status;
        response.ReviewNote = request.Note?.Trim();
        response.ReviewedByUserId = userId;
        response.ReviewedAt = DateTime.UtcNow;
        response.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(surveyId, userId, DataCollectionEventType.ResponseReviewed,
            $"Response {status}.", response.Id, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadResponseAsync(surveyId, response.Id, cancellationToken)).ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        var response = await LoadResponseAsync(surveyId, responseId, cancellationToken);

        var isOwner = FieldResearchAccess.IsOwner(survey, userId);
        if (!isOwner && !(response.SubmittedByUserId == userId && response.Status == SurveyResponseStatus.Draft))
        {
            throw new UnauthorizedAccessException(
                "Only the survey owner can delete a submitted response; collectors may delete only their own drafts.");
        }

        if (response.Status != SurveyResponseStatus.Draft && survey.ResponseCount > 0)
        {
            survey.ResponseCount -= 1;
        }

        Repository.RemoveResponse(response);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----------------------------------------------------

    private async Task<SurveyResponse> LoadResponseAsync(Guid surveyId, Guid responseId, CancellationToken cancellationToken)
    {
        var response = await Repository.GetResponseByIdAsync(responseId, cancellationToken);
        if (response is null || response.SurveyId != surveyId)
        {
            throw new NotFoundException("Survey response not found.");
        }

        return response;
    }

    private static bool CanSeeAllResponses(Survey survey, Guid userId)
    {
        if (FieldResearchAccess.IsOwner(survey, userId))
        {
            return true;
        }

        var assignment = FieldResearchAccess.ActiveAssignment(survey, userId);
        return assignment is not null
            && (assignment.Role == FieldAssignmentRole.Supervisor || assignment.Role == FieldAssignmentRole.Reviewer);
    }

    private static void EnsureCanEditResponse(Survey survey, SurveyResponse response, Guid userId)
    {
        if (FieldResearchAccess.IsOwner(survey, userId) || response.SubmittedByUserId == userId)
        {
            return;
        }

        throw new UnauthorizedAccessException("You can only edit responses you collected.");
    }

    private static List<SurveyResponseAnswer> BuildAnswers(
        List<SurveyAnswerInputDto> inputs, List<SurveyQuestion> questions)
    {
        var questionIds = questions.Select(q => q.Id).ToHashSet();
        var result = new Dictionary<Guid, SurveyResponseAnswer>();

        foreach (var input in inputs)
        {
            if (!questionIds.Contains(input.SurveyQuestionId))
            {
                throw new ConflictException("An answer references a question that does not belong to this survey.");
            }

            result[input.SurveyQuestionId] = new SurveyResponseAnswer
            {
                Id = Guid.NewGuid(),
                SurveyQuestionId = input.SurveyQuestionId,
                ValueText = input.ValueText?.Trim(),
                ValueNumber = input.ValueNumber,
                ValueBoolean = input.ValueBoolean,
                ValueDate = input.ValueDate,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
            };
        }

        return result.Values.ToList();
    }

    private static void EnsureRequiredAnswered(List<SurveyQuestion> questions, IEnumerable<SurveyResponseAnswer> answers)
    {
        var answered = answers
            .Where(a => a.ValueText is not null || a.ValueNumber is not null || a.ValueBoolean is not null
                || a.ValueDate is not null || a.Latitude is not null)
            .Select(a => a.SurveyQuestionId)
            .ToHashSet();

        var missing = questions.Where(q => q.IsRequired && !answered.Contains(q.Id)).ToList();
        if (missing.Count > 0)
        {
            throw new ConflictException(
                $"{missing.Count} required question(s) have no answer: " +
                string.Join("; ", missing.Take(5).Select(q => q.Text)));
        }
    }

    private static FieldResponseSource? ParseSource(string? value)
        => Enum.TryParse<FieldResponseSource>(value, true, out var parsed) ? parsed : null;
}
