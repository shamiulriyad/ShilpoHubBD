using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Apprenticeship;
using ShilpoHubBD.Domain.Entities.BusinessPartner;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Employment;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Repositories;

public class PolicySimulationRepository : IPolicySimulationRepository
{
    private static readonly OrderStatus[] CountedOrderStatuses =
    {
        OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Shipped,
        OrderStatus.Delivered, OrderStatus.ReturnRequested,
    };

    private readonly ShilpoHubDbContext _context;

    public PolicySimulationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<GovScopeRef?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken)
        => await _context.Districts
            .Where(d => d.Id == districtId)
            .Select(d => new GovScopeRef(d.Id, d.Name, d.Division))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<GovVillageRef?> GetVillageAsync(Guid villageId, CancellationToken cancellationToken)
        => await _context.Villages
            .Where(v => v.Id == villageId)
            .Select(v => new GovVillageRef(v.Id, v.Name, v.Craft, v.DistrictId, v.District.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PolicyBaselineSignals> GatherBaselineAsync(
        HeritageIndexScope scope, Guid? scopeId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        Guid? districtId = scope switch
        {
            HeritageIndexScope.District => scopeId,
            HeritageIndexScope.Village => scopeId.HasValue
                ? await _context.Villages.Where(v => v.Id == scopeId.Value)
                    .Select(v => (Guid?)v.DistrictId).FirstOrDefaultAsync(cancellationToken)
                : null,
            _ => null,
        };

        // ---- Producers -------------------------------------------
        int producers, activeProducers;
        if (districtId.HasValue)
        {
            var ids = _context.Products
                .Where(p => p.DistrictId == districtId.Value)
                .Select(p => p.ProducerId)
                .Distinct();
            producers = await ids.CountAsync(cancellationToken);
            activeProducers = await _context.Users.CountAsync(u => ids.Contains(u.Id) && u.IsActive, cancellationToken);
        }
        else
        {
            var producerQuery = _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer));
            producers = await producerQuery.CountAsync(cancellationToken);
            activeProducers = await producerQuery.CountAsync(u => u.IsActive, cancellationToken);
        }

        // ---- Employment & pipeline (platform-wide levels) --------
        var employment = await _context.JobApplications
            .CountAsync(a => a.Status == JobApplicationStatus.Hired, cancellationToken);
        var apprentices = await _context.ApprenticeEnrollments
            .CountAsync(e => e.Status == ApprenticeEnrollmentStatus.Active, cancellationToken);

        // ---- Marketplace value in window -----------------------
        var orderItems = _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt < to);
        if (districtId.HasValue)
        {
            orderItems = orderItems.Where(i => i.Product.DistrictId == districtId.Value);
        }

        var marketplaceSales = await orderItems.Select(i => (decimal?)i.LineTotal).SumAsync(cancellationToken) ?? 0m;
        var orderCount = await orderItems.Select(i => i.OrderId).Distinct().CountAsync(cancellationToken);

        // ---- Export value in window ---------------------------
        var exporterUserIds = _context.BusinessPartnerProfiles
            .Where(p => p.BusinessType == BusinessType.Exporter)
            .Select(p => p.UserId);
        var exportItems = _context.OrderItems
            .Where(i => CountedOrderStatuses.Contains(i.Order.Status))
            .Where(i => i.Order.CreatedAt >= from && i.Order.CreatedAt < to)
            .Where(i => exporterUserIds.Contains(i.Order.UserId));
        if (districtId.HasValue)
        {
            exportItems = exportItems.Where(i => i.Product.DistrictId == districtId.Value);
        }

        var exportValue = await exportItems.Select(i => (decimal?)i.LineTotal).SumAsync(cancellationToken) ?? 0m;

        // ---- Tourism in window -------------------------------
        var bookings = _context.Bookings
            .Where(bk => bk.CreatedAt >= from && bk.CreatedAt < to);
        if (districtId.HasValue)
        {
            bookings = bookings.Where(bk => bk.Service.DistrictId == districtId.Value);
        }

        var completed = bookings.Where(bk => bk.Status == Domain.Entities.TouristBooking.BookingStatus.Completed);
        var tourismRevenue = await completed.Select(bk => (decimal?)bk.TotalPrice).SumAsync(cancellationToken) ?? 0m;
        var tourismBookings = await bookings.CountAsync(cancellationToken);

        return new PolicyBaselineSignals
        {
            Producers = producers,
            ActiveProducers = activeProducers,
            Employment = employment,
            ApprenticesInPipeline = apprentices,
            ExportValue = exportValue,
            TourismRevenue = tourismRevenue,
            MarketplaceSalesValue = marketplaceSales,
            EconomyValue = marketplaceSales + tourismRevenue,
            AverageOrderValue = orderCount == 0 ? 0m : Math.Round(marketplaceSales / orderCount, 2),
            TourismBookings = tourismBookings,
        };
    }

    public async Task AddAsync(PolicySimulation simulation, CancellationToken cancellationToken)
        => await _context.PolicySimulations.AddAsync(simulation, cancellationToken);

    public void Remove(PolicySimulation simulation)
        => _context.PolicySimulations.Remove(simulation);

    public Task<PolicySimulation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.PolicySimulations
            .Include(s => s.GeneratedBy)
            .Include(s => s.Projections)
            .Include(s => s.Recommendations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(List<PolicySimulation> Items, int TotalCount)> GetPagedAsync(
        PolicySimulationQueryParameters query, CancellationToken cancellationToken)
    {
        var sims = _context.PolicySimulations
            .Include(s => s.GeneratedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SimulationType)
            && Enum.TryParse<PolicySimulationType>(query.SimulationType, true, out var type))
        {
            sims = sims.Where(s => s.SimulationType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Scope)
            && Enum.TryParse<HeritageIndexScope>(query.Scope, true, out var scope))
        {
            sims = sims.Where(s => s.Scope == scope);
        }

        if (query.ScopeId.HasValue)
        {
            sims = sims.Where(s => s.ScopeId == query.ScopeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<PolicySimulationStatus>(query.Status, true, out var status))
        {
            sims = sims.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            sims = sims.Where(s => s.Title.ToLower().Contains(term) || s.ScopeLabel.ToLower().Contains(term));
        }

        sims = sims.OrderByDescending(s => s.CreatedAt);

        var totalCount = await sims.CountAsync(cancellationToken);
        var items = await sims
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
