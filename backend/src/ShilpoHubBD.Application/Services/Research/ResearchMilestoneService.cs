using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchMilestoneService : ResearchServiceBase, IResearchMilestoneService
{
    public ResearchMilestoneService(IResearchProjectRepository repository) : base(repository)
    {
    }

    public async Task<List<ResearchMilestoneDto>> GetForProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);

        var milestones = await Repository.GetMilestonesAsync(projectId, cancellationToken);
        var result = new List<ResearchMilestoneDto>(milestones.Count);
        foreach (var milestone in milestones)
        {
            var taskCount = await Repository.CountTasksForMilestoneAsync(milestone.Id, cancellationToken);
            result.Add(milestone.ToDto(taskCount));
        }

        return result;
    }

    public async Task<ResearchMilestoneDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchMilestoneRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);

        var now = DateTime.UtcNow;
        var milestone = new ResearchMilestone
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = ResearchMilestoneStatus.Planned,
            TargetDate = request.TargetDate,
            OrderIndex = request.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddMilestoneAsync(milestone, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.MilestoneCreated,
            $"{member.User?.FullName} added milestone \"{milestone.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return milestone.ToDto(0);
    }

    public async Task<ResearchMilestoneDto> UpdateAsync(
        Guid userId, Guid projectId, Guid milestoneId, UpdateResearchMilestoneRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var milestone = await LoadMilestoneAsync(projectId, milestoneId, cancellationToken);

        if (!Enum.TryParse<ResearchMilestoneStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Planned, InProgress, Achieved, Missed.");
        }

        var statusChanged = milestone.Status != status;

        milestone.Title = request.Title.Trim();
        milestone.Description = request.Description?.Trim();
        milestone.Status = status;
        milestone.TargetDate = request.TargetDate;
        milestone.OrderIndex = request.OrderIndex;
        milestone.AchievedAt = status == ResearchMilestoneStatus.Achieved
            ? milestone.AchievedAt ?? DateTime.UtcNow
            : null;
        milestone.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId,
            statusChanged ? ResearchActivityType.MilestoneStatusChanged : ResearchActivityType.MilestoneUpdated,
            statusChanged
                ? $"{member.User?.FullName} set milestone \"{milestone.Title}\" to {status}."
                : $"{member.User?.FullName} updated milestone \"{milestone.Title}\".",
            cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        var taskCount = await Repository.CountTasksForMilestoneAsync(milestone.Id, cancellationToken);
        return milestone.ToDto(taskCount);
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid milestoneId, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var milestone = await LoadMilestoneAsync(projectId, milestoneId, cancellationToken);

        Repository.RemoveMilestone(milestone);
        await AddActivityAsync(projectId, userId, ResearchActivityType.MilestoneDeleted,
            $"{member.User?.FullName} deleted milestone \"{milestone.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ResearchMilestone> LoadMilestoneAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken)
    {
        var milestone = await Repository.GetMilestoneByIdAsync(milestoneId, cancellationToken);
        if (milestone is null || milestone.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Milestone not found.");
        }

        return milestone;
    }
}
