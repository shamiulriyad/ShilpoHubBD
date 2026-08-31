using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchTaskService : ResearchServiceBase, IResearchTaskService
{
    private readonly IUserRepository _userRepository;

    public ResearchTaskService(IResearchProjectRepository repository, IUserRepository userRepository)
        : base(repository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ResearchTaskDto>> GetForProjectAsync(
        Guid userId, Guid projectId, ResearchTaskQueryParameters query, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);

        var (items, totalCount) = await Repository.GetTasksAsync(projectId, query, cancellationToken);
        return new PagedResult<ResearchTaskDto>
        {
            Items = items.Select(t => t.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ResearchTaskDto> GetByIdAsync(
        Guid userId, Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);
        var task = await LoadTaskAsync(projectId, taskId, cancellationToken);
        return task.ToDto();
    }

    public async Task<ResearchTaskDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchTaskRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);

        var priority = ParsePriority(request.Priority) ?? ResearchTaskPriority.Medium;
        await ValidateMilestoneAsync(projectId, request.MilestoneId, cancellationToken);
        await ValidateAssigneeAsync(projectId, request.AssignedToUserId, cancellationToken);

        var now = DateTime.UtcNow;
        var task = new ResearchTask
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            MilestoneId = request.MilestoneId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = ResearchTaskStatus.Todo,
            Priority = priority,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = userId,
            DueDate = request.DueDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddTaskAsync(task, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.TaskCreated,
            $"{member.User?.FullName} created task \"{task.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadTaskAsync(projectId, task.Id, cancellationToken)).ToDto();
    }

    public async Task<ResearchTaskDto> UpdateAsync(
        Guid userId, Guid projectId, Guid taskId, UpdateResearchTaskRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var task = await LoadTaskAsync(projectId, taskId, cancellationToken);
        EnsureCanMutateTask(task, member);

        var priority = ParsePriority(request.Priority)
            ?? throw new ConflictException("Priority must be one of: Low, Medium, High, Critical.");
        await ValidateMilestoneAsync(projectId, request.MilestoneId, cancellationToken);
        await ValidateAssigneeAsync(projectId, request.AssignedToUserId, cancellationToken);

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = priority;
        task.MilestoneId = request.MilestoneId;
        task.AssignedToUserId = request.AssignedToUserId;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.TaskUpdated,
            $"{member.User?.FullName} updated task \"{task.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadTaskAsync(projectId, task.Id, cancellationToken)).ToDto();
    }

    public async Task<ResearchTaskDto> UpdateStatusAsync(
        Guid userId, Guid projectId, Guid taskId, UpdateResearchTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var task = await LoadTaskAsync(projectId, taskId, cancellationToken);
        EnsureCanMutateTask(task, member);

        if (!Enum.TryParse<ResearchTaskStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Todo, InProgress, Blocked, Done, Cancelled.");
        }

        task.Status = status;
        task.CompletedAt = status == ResearchTaskStatus.Done ? DateTime.UtcNow : null;
        task.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.TaskStatusChanged,
            $"{member.User?.FullName} moved task \"{task.Title}\" to {status}.", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadTaskAsync(projectId, task.Id, cancellationToken)).ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var task = await LoadTaskAsync(projectId, taskId, cancellationToken);

        if (task.CreatedByUserId != userId && !member.Role.AtLeast(ResearchRole.Admin))
        {
            throw new UnauthorizedAccessException("Only the task creator or a project admin can delete this task.");
        }

        Repository.RemoveTask(task);
        await AddActivityAsync(projectId, userId, ResearchActivityType.TaskDeleted,
            $"{member.User?.FullName} deleted task \"{task.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----------------------------------------------------------

    private async Task<ResearchTask> LoadTaskAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await Repository.GetTaskByIdAsync(taskId, cancellationToken);
        if (task is null || task.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Research task not found.");
        }

        return task;
    }

    private static void EnsureCanMutateTask(ResearchTask task, ResearchProjectMember member)
    {
        if (member.Role.AtLeast(ResearchRole.Researcher))
        {
            return;
        }

        // Contributors may only change tasks they created or that are assigned to them.
        if (task.CreatedByUserId != member.UserId && task.AssignedToUserId != member.UserId)
        {
            throw new UnauthorizedAccessException(
                "Contributors can only modify tasks they created or that are assigned to them.");
        }
    }

    private async Task ValidateMilestoneAsync(Guid projectId, Guid? milestoneId, CancellationToken cancellationToken)
    {
        if (!milestoneId.HasValue)
        {
            return;
        }

        var milestone = await Repository.GetMilestoneByIdAsync(milestoneId.Value, cancellationToken);
        if (milestone is null || milestone.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Milestone not found in this project.");
        }
    }

    private async Task ValidateAssigneeAsync(Guid projectId, Guid? assigneeId, CancellationToken cancellationToken)
    {
        if (!assigneeId.HasValue)
        {
            return;
        }

        var membership = await Repository.GetMembershipAsync(projectId, assigneeId.Value, cancellationToken);
        if (membership is null)
        {
            throw new ConflictException("Tasks can only be assigned to project members.");
        }
    }

    private static ResearchTaskPriority? ParsePriority(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ResearchTaskPriority>(value, true, out var parsed) ? parsed : null;
    }
}
