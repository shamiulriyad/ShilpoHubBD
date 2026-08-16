using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageFestivalRepository : IHeritageFestivalRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageFestivalRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<HeritageFestival> WithDetails()
        => _context.HeritageFestivals.Include(f => f.District).Include(f => f.HeritagePlace).AsSplitQuery();

    public async Task<(List<HeritageFestival> Items, int TotalCount)> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken)
    {
        var festivals = WithDetails();

        if (query.ActiveOnly)
        {
            festivals = festivals.Where(f => f.IsActive);
        }

        if (query.DistrictId.HasValue)
        {
            festivals = festivals.Where(f => f.DistrictId == query.DistrictId.Value);
        }

        if (query.HeritagePlaceId.HasValue)
        {
            festivals = festivals.Where(f => f.HeritagePlaceId == query.HeritagePlaceId.Value);
        }

        if (query.FromDate.HasValue)
        {
            festivals = festivals.Where(f => f.EndDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            festivals = festivals.Where(f => f.StartDate <= query.ToDate.Value);
        }

        festivals = festivals.OrderBy(f => f.StartDate);

        var totalCount = await festivals.CountAsync(cancellationToken);
        var items = await festivals
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<HeritageFestival?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task AddAsync(HeritageFestival festival, CancellationToken cancellationToken)
        => await _context.HeritageFestivals.AddAsync(festival, cancellationToken);

    public void Remove(HeritageFestival festival)
        => _context.HeritageFestivals.Remove(festival);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
