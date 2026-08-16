using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Repositories;

public class TouristServiceRepository : ITouristServiceRepository
{
    private readonly ShilpoHubDbContext _context;

    public TouristServiceRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<TouristService> WithDetails()
        => _context.TouristServices.Include(s => s.Producer).Include(s => s.District).AsSplitQuery();

    public async Task<(List<TouristService> Items, int TotalCount)> GetPagedAsync(
        TouristServiceQueryParameters query, CancellationToken cancellationToken)
    {
        var services = WithDetails().Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            services = services.Where(s => EF.Functions.ILike(s.Title, $"%{search}%") || EF.Functions.ILike(s.Description, $"%{search}%"));
        }

        if (query.Type.HasValue)
        {
            services = services.Where(s => s.Type == query.Type.Value);
        }

        if (query.DistrictId.HasValue)
        {
            services = services.Where(s => s.DistrictId == query.DistrictId.Value);
        }

        services = services.OrderBy(s => s.Title);

        var totalCount = await services.CountAsync(cancellationToken);
        var items = await services
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<TouristService> Items, int TotalCount)> GetPagedByProducerAsync(
        Guid producerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var services = WithDetails().Where(s => s.ProducerId == producerId).OrderByDescending(s => s.CreatedAt);

        var totalCount = await services.CountAsync(cancellationToken);
        var items = await services
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<TouristService?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(TouristService service, CancellationToken cancellationToken)
        => await _context.TouristServices.AddAsync(service, cancellationToken);

    public void Remove(TouristService service)
        => _context.TouristServices.Remove(service);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
