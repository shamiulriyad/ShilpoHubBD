using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class PickupRequestRepository : IPickupRequestRepository
{
    private readonly ShilpoHubDbContext _context;

    public PickupRequestRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PickupRequest request, CancellationToken cancellationToken)
        => await _context.PickupRequests.AddAsync(request, cancellationToken);

    public void Remove(PickupRequest request) => _context.PickupRequests.Remove(request);

    public Task<PickupRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.PickupRequests
            .Include(r => r.Profile)
            .Include(r => r.RequestedBy)
            .Include(r => r.Order)
            .Include(r => r.OriginDistrict)
            .Include(r => r.OriginProducer)
            .Include(r => r.DestinationDistrict)
            .Include(r => r.Items)
            .Include(r => r.Events).ThenInclude(e => e.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken)
        => _context.PickupRequests.AnyAsync(r => r.ReferenceCode == referenceCode, cancellationToken);

    public async Task<(List<PickupRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, PickupRequestQueryParameters query, CancellationToken cancellationToken)
    {
        var requests = _context.PickupRequests
            .Include(r => r.Order)
            .Include(r => r.OriginDistrict)
            .AsQueryable();

        if (profileId.HasValue)
        {
            requests = requests.Where(r => r.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<PickupRequestStatus>(query.Status, true, out var status))
        {
            requests = requests.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority)
            && Enum.TryParse<PickupPriority>(query.Priority, true, out var priority))
        {
            requests = requests.Where(r => r.Priority == priority);
        }

        if (query.OrderId.HasValue)
        {
            requests = requests.Where(r => r.OrderId == query.OrderId.Value);
        }

        if (query.OriginProducerUserId.HasValue)
        {
            requests = requests.Where(r => r.OriginProducerUserId == query.OriginProducerUserId.Value);
        }

        if (query.OriginDistrictId.HasValue)
        {
            requests = requests.Where(r => r.OriginDistrictId == query.OriginDistrictId.Value);
        }

        if (query.ScheduledFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.ScheduledFrom.Value, DateTimeKind.Utc);
            requests = requests.Where(r => r.ScheduledPickupAt >= from);
        }

        if (query.ScheduledTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.ScheduledTo.Value, DateTimeKind.Utc);
            requests = requests.Where(r => r.ScheduledPickupAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            requests = requests.Where(r =>
                r.ReferenceCode.ToLower().Contains(term)
                || r.OriginContactName.ToLower().Contains(term)
                || r.OriginCity.ToLower().Contains(term));
        }

        requests = requests
            .OrderByDescending(r => r.Status == PickupRequestStatus.Draft
                || r.Status == PickupRequestStatus.Scheduled
                || r.Status == PickupRequestStatus.Assigned
                || r.Status == PickupRequestStatus.EnRoute)
            .ThenBy(r => r.ScheduledPickupAt ?? DateTime.MaxValue)
            .ThenByDescending(r => r.CreatedAt);

        var totalCount = await requests.CountAsync(cancellationToken);
        var items = await requests
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    public Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.FirstOrDefaultAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
