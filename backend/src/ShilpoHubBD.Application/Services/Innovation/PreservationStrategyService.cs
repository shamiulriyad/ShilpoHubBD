using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Services.Innovation;

public class PreservationStrategyService : IPreservationStrategyService
{
    private readonly IPreservationStrategyRepository _repository;
    private readonly IInnovationLinkResolver _links;

    public PreservationStrategyService(IPreservationStrategyRepository repository, IInnovationLinkResolver links)
    {
        _repository = repository;
        _links = links;
    }

    public async Task<PagedResult<PreservationStrategyListItemDto>> GetMineAsync(
        Guid userId, PreservationStrategyQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedForOwnerAsync(userId, query, cancellationToken);
        return new PagedResult<PreservationStrategyListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PreservationStrategyDetailDto> GetByIdAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken)
        => (await LoadOwnedDetailAsync(userId, strategyId, cancellationToken)).ToDetailDto();

    public async Task<PreservationStrategyDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreatePreservationStrategyRequest request, CancellationToken cancellationToken)
    {
        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.HeritageDatasetId, cancellationToken);
        ValidateWindow(request.StartDate, request.TargetDate);

        var now = DateTime.UtcNow;
        var strategy = new PreservationStrategy
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            ResearchProjectId = request.ResearchProjectId,
            HeritageDatasetId = request.HeritageDatasetId,
            Title = request.Title.Trim(),
            HeritageProblem = request.HeritageProblem.Trim(),
            ProposedSolution = request.ProposedSolution.Trim(),
            ExpectedImpact = request.ExpectedImpact?.Trim(),
            EvidenceReferences = request.EvidenceReferences?.Trim(),
            Status = PreservationStrategyStatus.Draft,
            StartDate = request.StartDate,
            TargetDate = request.TargetDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(strategy, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetDetailAsync(strategy.Id, cancellationToken))!.ToDetailDto();
    }

    public async Task<PreservationStrategyDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid strategyId, UpdatePreservationStrategyRequest request, CancellationToken cancellationToken)
    {
        var strategy = await LoadOwnedDetailAsync(userId, strategyId, cancellationToken);

        if (!Enum.TryParse<PreservationStrategyStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status is not a valid preservation strategy status.");
        }

        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.HeritageDatasetId, cancellationToken);
        ValidateWindow(request.StartDate, request.TargetDate);

        strategy.Title = request.Title.Trim();
        strategy.HeritageProblem = request.HeritageProblem.Trim();
        strategy.ProposedSolution = request.ProposedSolution.Trim();
        strategy.ExpectedImpact = request.ExpectedImpact?.Trim();
        strategy.EvidenceReferences = request.EvidenceReferences?.Trim();
        strategy.Status = status;
        strategy.ResearchProjectId = request.ResearchProjectId;
        strategy.HeritageDatasetId = request.HeritageDatasetId;
        strategy.StartDate = request.StartDate;
        strategy.TargetDate = request.TargetDate;
        strategy.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return strategy.ToDetailDto();
    }

    public async Task DeleteAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken)
    {
        var strategy = await _repository.GetByIdAsync(strategyId, cancellationToken)
            ?? throw new NotFoundException("Preservation strategy not found.");
        RequireOwner(strategy, userId);

        _repository.Remove(strategy);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- objectives ----

    public async Task<StrategyObjectiveDto> AddObjectiveAsync(
        Guid userId, Guid strategyId, CreateStrategyObjectiveRequest request, CancellationToken cancellationToken)
    {
        var strategy = await LoadOwnedAsync(userId, strategyId, cancellationToken);

        var now = DateTime.UtcNow;
        var objective = new StrategyObjective
        {
            Id = Guid.NewGuid(),
            PreservationStrategyId = strategyId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            OrderIndex = request.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddObjectiveAsync(objective, cancellationToken);
        strategy.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return objective.ToDto();
    }

    public async Task<StrategyObjectiveDto> UpdateObjectiveAsync(
        Guid userId, Guid strategyId, Guid objectiveId, UpdateStrategyObjectiveRequest request, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, strategyId, cancellationToken);
        var objective = await _repository.GetObjectiveByIdAsync(objectiveId, cancellationToken);
        if (objective is null || objective.PreservationStrategyId != strategyId)
        {
            throw new NotFoundException("Objective not found.");
        }

        objective.Title = request.Title.Trim();
        objective.Description = request.Description?.Trim();
        objective.OrderIndex = request.OrderIndex;
        if (request.IsAchieved && !objective.IsAchieved)
        {
            objective.AchievedAt = DateTime.UtcNow;
        }
        else if (!request.IsAchieved)
        {
            objective.AchievedAt = null;
        }

        objective.IsAchieved = request.IsAchieved;
        objective.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return objective.ToDto();
    }

    public async Task DeleteObjectiveAsync(Guid userId, Guid strategyId, Guid objectiveId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, strategyId, cancellationToken);
        var objective = await _repository.GetObjectiveByIdAsync(objectiveId, cancellationToken);
        if (objective is null || objective.PreservationStrategyId != strategyId)
        {
            throw new NotFoundException("Objective not found.");
        }

        _repository.RemoveObjective(objective);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- actions ----

    public async Task<StrategyActionDto> AddActionAsync(
        Guid userId, Guid strategyId, CreateStrategyActionRequest request, CancellationToken cancellationToken)
    {
        var strategy = await LoadOwnedAsync(userId, strategyId, cancellationToken);
        await ValidateObjectiveAsync(strategyId, request.StrategyObjectiveId, cancellationToken);
        await ValidateAssigneeAsync(request.AssignedToUserId, cancellationToken);
        ValidateWindow(request.StartDate, request.DueDate);

        var now = DateTime.UtcNow;
        var action = new StrategyAction
        {
            Id = Guid.NewGuid(),
            PreservationStrategyId = strategyId,
            StrategyObjectiveId = request.StrategyObjectiveId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = StrategyActionStatus.Planned,
            OrderIndex = request.OrderIndex,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddActionAsync(action, cancellationToken);
        strategy.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetActionByIdAsync(action.Id, cancellationToken))!.ToDto();
    }

    public async Task<StrategyActionDto> UpdateActionAsync(
        Guid userId, Guid strategyId, Guid actionId, UpdateStrategyActionRequest request, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, strategyId, cancellationToken);
        var action = await _repository.GetActionByIdAsync(actionId, cancellationToken);
        if (action is null || action.PreservationStrategyId != strategyId)
        {
            throw new NotFoundException("Action not found.");
        }

        if (!Enum.TryParse<StrategyActionStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Planned, InProgress, Done, Blocked, Cancelled.");
        }

        await ValidateObjectiveAsync(strategyId, request.StrategyObjectiveId, cancellationToken);
        await ValidateAssigneeAsync(request.AssignedToUserId, cancellationToken);
        ValidateWindow(request.StartDate, request.DueDate);

        action.StrategyObjectiveId = request.StrategyObjectiveId;
        action.Title = request.Title.Trim();
        action.Description = request.Description?.Trim();
        action.Status = status;
        action.OrderIndex = request.OrderIndex;
        action.StartDate = request.StartDate;
        action.DueDate = request.DueDate;
        action.AssignedToUserId = request.AssignedToUserId;
        action.CompletedAt = status == StrategyActionStatus.Done ? action.CompletedAt ?? DateTime.UtcNow : null;
        action.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetActionByIdAsync(action.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteActionAsync(Guid userId, Guid strategyId, Guid actionId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(userId, strategyId, cancellationToken);
        var action = await _repository.GetActionByIdAsync(actionId, cancellationToken);
        if (action is null || action.PreservationStrategyId != strategyId)
        {
            throw new NotFoundException("Action not found.");
        }

        _repository.RemoveAction(action);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----

    private async Task<PreservationStrategy> LoadOwnedAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken)
    {
        var strategy = await _repository.GetByIdAsync(strategyId, cancellationToken)
            ?? throw new NotFoundException("Preservation strategy not found.");
        RequireOwner(strategy, userId);
        return strategy;
    }

    private async Task<PreservationStrategy> LoadOwnedDetailAsync(Guid userId, Guid strategyId, CancellationToken cancellationToken)
    {
        var strategy = await _repository.GetDetailAsync(strategyId, cancellationToken)
            ?? throw new NotFoundException("Preservation strategy not found.");
        RequireOwner(strategy, userId);
        return strategy;
    }

    private static void RequireOwner(PreservationStrategy strategy, Guid userId)
    {
        if (strategy.OwnerUserId != userId)
        {
            throw new NotFoundException("Preservation strategy not found.");
        }
    }

    private async Task ValidateLinksAsync(
        Guid userId, bool isResearcher, Guid? projectId, Guid? datasetId, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await _links.IsResearchProjectMemberAsync(projectId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a research project you belong to.");
        }

        if (datasetId.HasValue && !await _links.IsDatasetAccessibleAsync(datasetId.Value, userId, isResearcher, cancellationToken))
        {
            throw new ConflictException("You do not have access to the linked dataset.");
        }
    }

    private async Task ValidateObjectiveAsync(Guid strategyId, Guid? objectiveId, CancellationToken cancellationToken)
    {
        if (!objectiveId.HasValue)
        {
            return;
        }

        var objective = await _repository.GetObjectiveByIdAsync(objectiveId.Value, cancellationToken);
        if (objective is null || objective.PreservationStrategyId != strategyId)
        {
            throw new NotFoundException("Objective not found in this strategy.");
        }
    }

    private async Task ValidateAssigneeAsync(Guid? assigneeId, CancellationToken cancellationToken)
    {
        if (assigneeId.HasValue && !await _links.UserExistsAsync(assigneeId.Value, cancellationToken))
        {
            throw new NotFoundException("Assigned user not found.");
        }
    }

    private static void ValidateWindow(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && end.Value < start.Value)
        {
            throw new ConflictException("End date cannot be earlier than start date.");
        }
    }
}
