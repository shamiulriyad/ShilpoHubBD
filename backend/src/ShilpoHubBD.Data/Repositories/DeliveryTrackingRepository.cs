using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class DeliveryTrackingRepository : IDeliveryTrackingRepository
{
    private readonly ShilpoHubDbContext _context;

    public DeliveryTrackingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken)
        => await _context.Shipments.AddAsync(shipment, cancellationToken);

    public void Remove(Shipment shipment) => _context.Shipments.Remove(shipment);

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Shipments
            .Include(s => s.Profile)
            .Include(s => s.CreatedBy)
            .Include(s => s.Order)
            .Include(s => s.PickupRequest)
            .Include(s => s.DeliveryRoute)
            .Include(s => s.OriginDistrict)
            .Include(s => s.DestinationDistrict)
            .Include(s => s.Events).ThenInclude(e => e.District)
            .Include(s => s.Events).ThenInclude(e => e.RecordedBy)
            .Include(s => s.Attempts).ThenInclude(a => a.RecordedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken)
        => _context.Shipments
            .Include(s => s.OriginDistrict)
            .Include(s => s.DestinationDistrict)
            .Include(s => s.Events).ThenInclude(e => e.District)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, cancellationToken);

    public Task<bool> TrackingNumberExistsAsync(string trackingNumber, CancellationToken cancellationToken)
        => _context.Shipments.AnyAsync(s => s.TrackingNumber == trackingNumber, cancellationToken);

    public async Task<(List<Shipment> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, ShipmentQueryParameters query, CancellationToken cancellationToken)
    {
        var shipments = _context.Shipments
            .Include(s => s.Order)
            .Include(s => s.DestinationDistrict)
            .AsQueryable();

        if (profileId.HasValue)
        {
            shipments = shipments.Where(s => s.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<ShipmentStatus>(query.Status, true, out var status))
        {
            shipments = shipments.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.ServiceLevel)
            && Enum.TryParse<ShipmentServiceLevel>(query.ServiceLevel, true, out var serviceLevel))
        {
            shipments = shipments.Where(s => s.ServiceLevel == serviceLevel);
        }

        if (query.OrderId.HasValue)
        {
            shipments = shipments.Where(s => s.OrderId == query.OrderId.Value);
        }

        if (query.DeliveryRouteId.HasValue)
        {
            shipments = shipments.Where(s => s.DeliveryRouteId == query.DeliveryRouteId.Value);
        }

        if (query.DestinationDistrictId.HasValue)
        {
            shipments = shipments.Where(s => s.DestinationDistrictId == query.DestinationDistrictId.Value);
        }

        if (query.IsCashOnDelivery.HasValue)
        {
            shipments = shipments.Where(s => s.IsCashOnDelivery == query.IsCashOnDelivery.Value);
        }

        if (query.Overdue == true)
        {
            var now = DateTime.UtcNow;
            shipments = shipments.Where(s =>
                s.EstimatedDeliveryAt != null
                && s.EstimatedDeliveryAt < now
                && s.Status != ShipmentStatus.Delivered
                && s.Status != ShipmentStatus.Returned
                && s.Status != ShipmentStatus.Cancelled);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            shipments = shipments.Where(s => s.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            shipments = shipments.Where(s => s.CreatedAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            shipments = shipments.Where(s =>
                s.TrackingNumber.ToLower().Contains(term)
                || s.RecipientName.ToLower().Contains(term)
                || s.DestinationCity.ToLower().Contains(term));
        }

        shipments = shipments
            .OrderByDescending(s => s.Status != ShipmentStatus.Delivered
                && s.Status != ShipmentStatus.Returned
                && s.Status != ShipmentStatus.Cancelled)
            .ThenBy(s => s.EstimatedDeliveryAt ?? DateTime.MaxValue)
            .ThenByDescending(s => s.CreatedAt);

        var totalCount = await shipments.CountAsync(cancellationToken);
        var items = await shipments
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> PickupRequestBelongsToProfileAsync(
        Guid pickupRequestId, Guid profileId, CancellationToken cancellationToken)
        => _context.PickupRequests.AnyAsync(
            p => p.Id == pickupRequestId && p.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> RouteBelongsToProfileAsync(Guid routeId, Guid profileId, CancellationToken cancellationToken)
        => _context.DeliveryRoutes.AnyAsync(
            r => r.Id == routeId && r.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
