using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

public abstract class SurveyServiceBase
{
    protected readonly ISurveyRepository Repository;
    protected readonly IResearchProjectRepository ProjectRepository;

    protected SurveyServiceBase(ISurveyRepository repository, IResearchProjectRepository projectRepository)
    {
        Repository = repository;
        ProjectRepository = projectRepository;
    }

    protected async Task<Survey> LoadSurveyAsync(Guid surveyId, CancellationToken cancellationToken)
        => await Repository.GetByIdAsync(surveyId, cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

    protected async Task<Survey> LoadSurveyDetailAsync(Guid surveyId, CancellationToken cancellationToken)
        => await Repository.GetDetailAsync(surveyId, cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

    /// <summary>Owner, any (active or inactive) assignment, or a member of the linked research project.</summary>
    protected async Task EnsureReadAccessAsync(Survey survey, Guid userId, CancellationToken cancellationToken)
    {
        if (FieldResearchAccess.IsOwner(survey, userId) || FieldResearchAccess.AnyAssignment(survey, userId))
        {
            return;
        }

        if (survey.ResearchProjectId.HasValue)
        {
            var membership = await ProjectRepository.GetMembershipAsync(
                survey.ResearchProjectId.Value, userId, cancellationToken);
            if (membership is not null)
            {
                return;
            }
        }

        throw new NotFoundException("Survey not found.");
    }

    protected async Task AddEventAsync(
        Guid surveyId,
        Guid actorUserId,
        DataCollectionEventType type,
        string summary,
        Guid? responseId,
        CancellationToken cancellationToken)
    {
        await Repository.AddEventAsync(new DataCollectionEvent
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            ActorUserId = actorUserId,
            EventType = type,
            Summary = summary.Length > 500 ? summary[..500] : summary,
            SurveyResponseId = responseId,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
