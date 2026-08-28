using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Repositories;

public class PreservationStrategyRepository : IPreservationStrategyRepository
{
    private readonly ShilpoHubDbContext _context;

    public PreservationStrategyRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<PreservationStrategy?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.PreservationStrategies
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<PreservationStrategy?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.PreservationStrategies
            .Include(s => s.Owner)
            .Include(s => s.Objectives)
            .Include(s => s.Actions).ThenInclude(a => a.AssignedTo)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(List<PreservationStrategy> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, PreservationStrategyQueryParameters query, CancellationToken cancellationToken)
    {
        var strategies = _context.PreservationStrategies
            .Include(s => s.Owner)
            .Include(s => s.Objectives)
            .Include(s => s.Actions)
            .AsSplitQuery()
            .Where(s => s.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<PreservationStrategyStatus>(query.Status, true, out var status))
        {
            strategies = strategies.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            strategies = strategies.Where(s => s.Title.ToLower().Contains(term)
                || s.HeritageProblem.ToLower().Contains(term));
        }

        strategies = strategies.OrderByDescending(s => s.UpdatedAt);

        var totalCount = await strategies.CountAsync(cancellationToken);
        var items = await strategies
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(PreservationStrategy strategy, CancellationToken cancellationToken)
        => await _context.PreservationStrategies.AddAsync(strategy, cancellationToken);

    public void Remove(PreservationStrategy strategy)
        => _context.PreservationStrategies.Remove(strategy);

    public Task<StrategyObjective?> GetObjectiveByIdAsync(Guid objectiveId, CancellationToken cancellationToken)
        => _context.StrategyObjectives.FirstOrDefaultAsync(o => o.Id == objectiveId, cancellationToken);

    public Task<List<StrategyObjective>> GetObjectivesAsync(Guid strategyId, CancellationToken cancellationToken)
        => _context.StrategyObjectives
            .Where(o => o.PreservationStrategyId == strategyId)
            .OrderBy(o => o.OrderIndex).ThenBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddObjectiveAsync(StrategyObjective objective, CancellationToken cancellationToken)
        => await _context.StrategyObjectives.AddAsync(objective, cancellationToken);

    public void RemoveObjective(StrategyObjective objective)
        => _context.StrategyObjectives.Remove(objective);

    public Task<StrategyAction?> GetActionByIdAsync(Guid actionId, CancellationToken cancellationToken)
        => _context.StrategyActions
            .Include(a => a.AssignedTo)
            .FirstOrDefaultAsync(a => a.Id == actionId, cancellationToken);

    public Task<List<StrategyAction>> GetActionsAsync(Guid strategyId, CancellationToken cancellationToken)
        => _context.StrategyActions
            .Include(a => a.AssignedTo)
            .Where(a => a.PreservationStrategyId == strategyId)
            .OrderBy(a => a.OrderIndex).ThenBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddActionAsync(StrategyAction action, CancellationToken cancellationToken)
        => await _context.StrategyActions.AddAsync(action, cancellationToken);

    public void RemoveAction(StrategyAction action)
        => _context.StrategyActions.Remove(action);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
