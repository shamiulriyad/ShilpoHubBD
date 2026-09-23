using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Repositories;

public class SavedTourPlanRepository : ISavedTourPlanRepository
{
    private readonly ShilpoHubDbContext _context;

    public SavedTourPlanRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // The JSON columns are large (the route geometry alone is thousands of points), so the list
    // query never loads them.
    public async Task<(List<SavedTourPlan> Items, int TotalCount)> GetPagedForUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.SavedTourPlans.AsNoTracking().Where(p => p.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new SavedTourPlan
            {
                Id = p.Id, UserId = p.UserId, Title = p.Title, DistrictId = p.DistrictId, DistrictName = p.DistrictName,
                OriginText = p.OriginText, DurationDays = p.DurationDays, PartySize = p.PartySize, StartDate = p.StartDate,
                TransportMode = p.TransportMode, TotalEstimatedCost = p.TotalEstimatedCost, IsAiGenerated = p.IsAiGenerated,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<SavedTourPlan?> GetForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => _context.SavedTourPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

    public async Task AddAsync(SavedTourPlan plan, CancellationToken cancellationToken)
        => await _context.SavedTourPlans.AddAsync(plan, cancellationToken);

    public void Remove(SavedTourPlan plan) => _context.SavedTourPlans.Remove(plan);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
