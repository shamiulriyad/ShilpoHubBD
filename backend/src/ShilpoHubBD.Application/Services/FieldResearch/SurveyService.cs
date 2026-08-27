using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

public class SurveyService : SurveyServiceBase, ISurveyService
{
    private readonly IUserRepository _userRepository;

    public SurveyService(
        ISurveyRepository repository,
        IResearchProjectRepository projectRepository,
        IUserRepository userRepository)
        : base(repository, projectRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<SurveyListItemDto>> GetForUserAsync(
        Guid userId, SurveyQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await Repository.GetPagedForUserAsync(userId, query, cancellationToken);
        return new PagedResult<SurveyListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto(userId)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<SurveyDetailDto> GetByIdAsync(Guid userId, Guid surveyId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);
        return await BuildDetailAsync(survey, userId, cancellationToken);
    }

    public async Task<SurveyDetailDto> CreateAsync(Guid userId, CreateSurveyRequest request, CancellationToken cancellationToken)
    {
        await ValidateProjectLinkAsync(userId, request.ResearchProjectId, cancellationToken);
        ValidateWindow(request.OpensAt, request.ClosesAt);

        var now = DateTime.UtcNow;
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            ResearchProjectId = request.ResearchProjectId,
            Slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Objective = request.Objective?.Trim(),
            TargetRegion = request.TargetRegion?.Trim(),
            Language = string.IsNullOrWhiteSpace(request.Language) ? "bn" : request.Language.Trim(),
            Status = SurveyStatus.Draft,
            AllowAnonymousResponses = request.AllowAnonymousResponses,
            OpensAt = request.OpensAt,
            ClosesAt = request.ClosesAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddAsync(survey, cancellationToken);
        await AddEventAsync(survey.Id, userId, DataCollectionEventType.SurveyCreated,
            $"Survey \"{survey.Title}\" created.", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, survey.Id, cancellationToken);
    }

    public async Task<SurveyDetailDto> UpdateAsync(
        Guid userId, Guid surveyId, UpdateSurveyRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        await ValidateProjectLinkAsync(userId, request.ResearchProjectId, cancellationToken);
        ValidateWindow(request.OpensAt, request.ClosesAt);

        survey.Title = request.Title.Trim();
        survey.Description = request.Description.Trim();
        survey.Objective = request.Objective?.Trim();
        survey.TargetRegion = request.TargetRegion?.Trim();
        survey.Language = string.IsNullOrWhiteSpace(request.Language) ? survey.Language : request.Language.Trim();
        survey.AllowAnonymousResponses = request.AllowAnonymousResponses;
        survey.ResearchProjectId = request.ResearchProjectId;
        survey.OpensAt = request.OpensAt;
        survey.ClosesAt = request.ClosesAt;
        survey.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(surveyId, userId, DataCollectionEventType.SurveyUpdated,
            "Survey details updated.", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return await BuildDetailAsync(survey, userId, cancellationToken);
    }

    public async Task<SurveyDetailDto> UpdateStatusAsync(
        Guid userId, Guid surveyId, UpdateSurveyStatusRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        if (!Enum.TryParse<SurveyStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Draft, Active, Paused, Closed, Archived.");
        }

        if (status == SurveyStatus.Active && survey.Questions.Count == 0)
        {
            throw new ConflictException("A survey needs at least one question before it can be activated.");
        }

        if (survey.Status == status)
        {
            throw new ConflictException($"Survey is already {status}.");
        }

        survey.Status = status;
        survey.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(surveyId, userId, DataCollectionEventType.SurveyStatusChanged,
            $"Survey status changed to {status}.", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return await BuildDetailAsync(survey, userId, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid surveyId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyDetailAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        if (survey.ResponseCount > 0)
        {
            throw new ConflictException("Surveys with collected responses cannot be deleted. Archive it instead.");
        }

        Repository.Remove(survey);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    // ---- questions -----------------------------------------------------

    public async Task<SurveyQuestionDto> AddQuestionAsync(
        Guid userId, Guid surveyId, CreateSurveyQuestionRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var type = ParseQuestionType(request.QuestionType);
        ValidateQuestionRange(request.MinValue, request.MaxValue);

        var now = DateTime.UtcNow;
        var question = new SurveyQuestion
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            Text = request.Text.Trim(),
            HelpText = request.HelpText?.Trim(),
            QuestionType = type,
            IsRequired = request.IsRequired,
            OrderIndex = request.OrderIndex,
            OptionsJson = request.OptionsJson,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddQuestionAsync(question, cancellationToken);
        survey.UpdatedAt = now;
        await AddEventAsync(surveyId, userId, DataCollectionEventType.QuestionAdded,
            $"Question added: \"{Truncate(question.Text, 80)}\".", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return question.ToDto();
    }

    public async Task<SurveyQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid surveyId, Guid questionId, UpdateSurveyQuestionRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var question = await Repository.GetQuestionByIdAsync(questionId, cancellationToken);
        if (question is null || question.SurveyId != surveyId)
        {
            throw new NotFoundException("Survey question not found.");
        }

        var type = ParseQuestionType(request.QuestionType);
        ValidateQuestionRange(request.MinValue, request.MaxValue);

        if (question.QuestionType != type && await Repository.QuestionHasAnswersAsync(questionId, cancellationToken))
        {
            throw new ConflictException("The question type cannot be changed after responses have been collected.");
        }

        question.Text = request.Text.Trim();
        question.HelpText = request.HelpText?.Trim();
        question.QuestionType = type;
        question.IsRequired = request.IsRequired;
        question.OrderIndex = request.OrderIndex;
        question.OptionsJson = request.OptionsJson;
        question.MinValue = request.MinValue;
        question.MaxValue = request.MaxValue;
        question.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(surveyId, userId, DataCollectionEventType.QuestionUpdated,
            $"Question updated: \"{Truncate(question.Text, 80)}\".", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return question.ToDto();
    }

    public async Task DeleteQuestionAsync(Guid userId, Guid surveyId, Guid questionId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var question = await Repository.GetQuestionByIdAsync(questionId, cancellationToken);
        if (question is null || question.SurveyId != surveyId)
        {
            throw new NotFoundException("Survey question not found.");
        }

        if (await Repository.QuestionHasAnswersAsync(questionId, cancellationToken))
        {
            throw new ConflictException("This question already has answers and cannot be deleted.");
        }

        Repository.RemoveQuestion(question);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.QuestionRemoved,
            $"Question removed: \"{Truncate(question.Text, 80)}\".", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    // ---- field researchers -------------------------------------------

    public async Task<List<SurveyFieldAssignmentDto>> GetFieldResearchersAsync(
        Guid userId, Guid surveyId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        var assignments = await Repository.GetAssignmentsAsync(surveyId, cancellationToken);
        return assignments.Select(a => a.ToDto()).ToList();
    }

    public async Task<SurveyFieldAssignmentDto> AssignFieldResearcherAsync(
        Guid userId, Guid surveyId, AssignFieldResearcherRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var role = ParseAssignmentRole(request.Role) ?? FieldAssignmentRole.Collector;

        if (request.FieldResearcherUserId == survey.OwnerUserId)
        {
            throw new ConflictException("The survey owner is already responsible for the survey.");
        }

        var user = await _userRepository.GetByIdAsync(request.FieldResearcherUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var existing = await Repository.GetAssignmentAsync(surveyId, request.FieldResearcherUserId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("This field researcher is already assigned to the survey.");
        }

        var assignment = new SurveyFieldAssignment
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            FieldResearcherUserId = user.Id,
            Role = role,
            AreaNote = request.AreaNote?.Trim(),
            IsActive = true,
            AssignedByUserId = userId,
            AssignedAt = DateTime.UtcNow,
        };

        await Repository.AddAssignmentAsync(assignment, cancellationToken);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.FieldResearcherAssigned,
            $"{user.FullName} assigned as {role}.", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        assignment.FieldResearcher = user;
        return assignment.ToDto();
    }

    public async Task<SurveyFieldAssignmentDto> UpdateFieldAssignmentAsync(
        Guid userId, Guid surveyId, Guid assignmentId, UpdateFieldAssignmentRequest request, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var assignment = await Repository.GetAssignmentByIdAsync(assignmentId, cancellationToken);
        if (assignment is null || assignment.SurveyId != surveyId)
        {
            throw new NotFoundException("Field assignment not found.");
        }

        var role = ParseAssignmentRole(request.Role)
            ?? throw new ConflictException("Role must be one of: Collector, Supervisor, Reviewer.");

        assignment.Role = role;
        assignment.AreaNote = request.AreaNote?.Trim();
        assignment.IsActive = request.IsActive;

        await AddEventAsync(surveyId, userId, DataCollectionEventType.FieldResearcherUpdated,
            $"{assignment.FieldResearcher?.FullName} assignment updated ({role}, active={request.IsActive}).",
            null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return assignment.ToDto();
    }

    public async Task RemoveFieldResearcherAsync(
        Guid userId, Guid surveyId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        FieldResearchAccess.RequireOwner(survey, userId);

        var assignment = await Repository.GetAssignmentByIdAsync(assignmentId, cancellationToken);
        if (assignment is null || assignment.SurveyId != surveyId)
        {
            throw new NotFoundException("Field assignment not found.");
        }

        Repository.RemoveAssignment(assignment);
        await AddEventAsync(surveyId, userId, DataCollectionEventType.FieldResearcherRemoved,
            $"{assignment.FieldResearcher?.FullName} removed from the survey.", null, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DataCollectionEventDto>> GetHistoryAsync(
        Guid userId, Guid surveyId, int take, CancellationToken cancellationToken)
    {
        var survey = await LoadSurveyAsync(surveyId, cancellationToken);
        await EnsureReadAccessAsync(survey, userId, cancellationToken);

        take = Math.Clamp(take, 1, 200);
        var events = await Repository.GetEventsAsync(surveyId, take, cancellationToken);
        return events.Select(e => e.ToDto()).ToList();
    }

    // ---- helpers ----------------------------------------------------

    private async Task<SurveyDetailDto> BuildDetailAsync(Survey survey, Guid userId, CancellationToken cancellationToken)
    {
        var evidenceCount = (await Repository.GetEvidenceAsync(
            survey.Id, new FieldEvidenceQueryParameters { Page = 1, PageSize = 1 }, cancellationToken)).TotalCount;

        var canManage = FieldResearchAccess.IsOwner(survey, userId);
        var assignment = survey.FieldAssignments.FirstOrDefault(a => a.FieldResearcherUserId == userId);

        return new SurveyDetailDto
        {
            Id = survey.Id,
            Slug = survey.Slug,
            Title = survey.Title,
            Description = survey.Description,
            Objective = survey.Objective,
            TargetRegion = survey.TargetRegion,
            Language = survey.Language,
            Status = survey.Status.ToString(),
            AllowAnonymousResponses = survey.AllowAnonymousResponses,
            OwnerUserId = survey.OwnerUserId,
            OwnerName = survey.Owner?.FullName ?? string.Empty,
            ResearchProjectId = survey.ResearchProjectId,
            MyRole = canManage ? "Owner" : assignment?.Role.ToString() ?? "Reader",
            CanManage = canManage,
            OpensAt = survey.OpensAt,
            ClosesAt = survey.ClosesAt,
            ResponseCount = survey.ResponseCount,
            EvidenceCount = evidenceCount,
            CreatedAt = survey.CreatedAt,
            UpdatedAt = survey.UpdatedAt,
            Questions = survey.Questions
                .OrderBy(q => q.OrderIndex)
                .ThenBy(q => q.CreatedAt)
                .Select(q => q.ToDto())
                .ToList(),
            FieldAssignments = survey.FieldAssignments
                .OrderBy(a => a.AssignedAt)
                .Select(a => a.ToDto())
                .ToList(),
        };
    }

    private async Task ValidateProjectLinkAsync(Guid userId, Guid? projectId, CancellationToken cancellationToken)
    {
        if (!projectId.HasValue)
        {
            return;
        }

        var membership = await ProjectRepository.GetMembershipAsync(projectId.Value, userId, cancellationToken);
        if (membership is null)
        {
            throw new ConflictException("You can only link a survey to a research project you belong to.");
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var slug = baseSlug;
        var suffix = 1;
        while (await Repository.SlugExistsAsync(slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static void ValidateWindow(DateTime? opensAt, DateTime? closesAt)
    {
        if (opensAt.HasValue && closesAt.HasValue && closesAt.Value < opensAt.Value)
        {
            throw new ConflictException("ClosesAt cannot be earlier than OpensAt.");
        }
    }

    private static void ValidateQuestionRange(double? min, double? max)
    {
        if (min.HasValue && max.HasValue && max.Value < min.Value)
        {
            throw new ConflictException("MaxValue cannot be less than MinValue.");
        }
    }

    private static SurveyQuestionType ParseQuestionType(string value)
        => Enum.TryParse<SurveyQuestionType>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("QuestionType is not a valid survey question type.");

    private static FieldAssignmentRole? ParseAssignmentRole(string? value)
        => Enum.TryParse<FieldAssignmentRole>(value, true, out var parsed) ? parsed : null;

    private static string Truncate(string value, int max)
        => value.Length > max ? value[..max] : value;
}
