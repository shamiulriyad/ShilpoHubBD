using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Repositories;

public class ReturnHandlingRepository : IReturnHandlingRepository
{
    private readonly ShilpoHubDbContext _context;

    public ReturnHandlingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ReturnRequest returnRequest, CancellationToken cancellationToken)
        => await _context.ReturnRequests.AddAsync(returnRequest, cancellationToken);

    public void Remove(ReturnRequest returnRequest) => _context.ReturnRequests.Remove(returnRequest);

    public Task<ReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ReturnRequests
            .Include(r => r.Profile)
            .Include(r => r.CreatedBy)
            .Include(r => r.Shipment)
            .Include(r => r.Order)
            .Include(r => r.DestinationWarehouse)
            .Include(r => r.PickupDistrict)
            .Include(r => r.ApprovedBy)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.Inspections).ThenInclude(i => i.InspectedBy)
            .Include(r => r.Events).ThenInclude(e => e.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken)
        => _context.ReturnRequests.AnyAsync(r => r.ReferenceCode == referenceCode, cancellationToken);

    public async Task<(List<ReturnRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? profileId, ReturnRequestQueryParameters query, CancellationToken cancellationToken)
    {
        var returns = _context.ReturnRequests
            .Include(r => r.Items)
            .AsQueryable();

        if (profileId.HasValue)
        {
            returns = returns.Where(r => r.LogisticsPartnerProfileId == profileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<ReturnStatus>(query.Status, true, out var status))
        {
            returns = returns.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Reason)
            && Enum.TryParse<ReturnReason>(query.Reason, true, out var reason))
        {
            returns = returns.Where(r => r.Reason == reason);
        }

        if (query.ShipmentId.HasValue)
        {
            returns = returns.Where(r => r.ShipmentId == query.ShipmentId.Value);
        }

        if (query.OrderId.HasValue)
        {
            returns = returns.Where(r => r.OrderId == query.OrderId.Value);
        }

        if (query.DestinationWarehouseId.HasValue)
        {
            returns = returns.Where(r => r.DestinationWarehouseId == query.DestinationWarehouseId.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Utc);
            returns = returns.Where(r => r.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Utc);
            returns = returns.Where(r => r.CreatedAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            returns = returns.Where(r =>
                r.ReferenceCode.ToLower().Contains(term)
                || r.CustomerName.ToLower().Contains(term));
        }

        returns = returns
            .OrderByDescending(r => r.Status != ReturnStatus.Closed
                && r.Status != ReturnStatus.Cancelled
                && r.Status != ReturnStatus.Refunded)
            .ThenByDescending(r => r.CreatedAt);

        var totalCount = await returns.CountAsync(cancellationToken);
        var items = await returns
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ShipmentBelongsToProfileAsync(Guid shipmentId, Guid profileId, CancellationToken cancellationToken)
        => _context.Shipments.AnyAsync(
            s => s.Id == shipmentId && s.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> WarehouseBelongsToProfileAsync(Guid warehouseId, Guid profileId, CancellationToken cancellationToken)
        => _context.Warehouses.AnyAsync(
            w => w.Id == warehouseId && w.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken)
        => _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken)
        => _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
