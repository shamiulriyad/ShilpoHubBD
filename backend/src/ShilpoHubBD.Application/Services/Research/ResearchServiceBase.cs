using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

/// <summary>Shared plumbing for the child-entity services (tasks, milestones, notes, papers, publications).</summary>
public abstract class ResearchServiceBase
{
    protected readonly IResearchProjectRepository Repository;

    protected ResearchServiceBase(IResearchProjectRepository repository)
    {
        Repository = repository;
    }

    protected async Task<(ResearchProject Project, ResearchProjectMember? Membership)> LoadProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await Repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        return (project, membership);
    }

    protected async Task<ResearchProjectMember> LoadProjectWithRoleAsync(
        Guid userId, Guid projectId, ResearchRole minimumRole, CancellationToken cancellationToken)
    {
        var (_, membership) = await LoadProjectAsync(userId, projectId, cancellationToken);
        return ResearchAccess.RequireRole(membership, minimumRole);
    }

    protected async Task<ResearchProjectMember?> LoadProjectWithReadAccessAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var (project, membership) = await LoadProjectAsync(userId, projectId, cancellationToken);
        ResearchAccess.RequireReadAccess(project, membership);
        return membership;
    }

    protected async Task AddActivityAsync(
        Guid projectId, Guid actorUserId, ResearchActivityType type, string summary, CancellationToken cancellationToken)
    {
        await Repository.AddActivityAsync(new ResearchActivity
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            ActorUserId = actorUserId,
            Type = type,
            Summary = summary.Length > 500 ? summary[..500] : summary,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }
}
