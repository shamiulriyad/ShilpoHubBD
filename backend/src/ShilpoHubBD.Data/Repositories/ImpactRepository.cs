using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Impact;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Data.Repositories;

public class ImpactRepository : IImpactRepository
{
    private static readonly OrderStatus[] CompletedStatuses =
    {
        OrderStatus.Delivered,
        OrderStatus.ReturnRequested,
        OrderStatus.Returned,
        OrderStatus.Refunded,
    };

    private readonly ShilpoHubDbContext _context;

    public ImpactRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<ImpactStatsDto> GetImpactStatsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = _context.Orders
            .Where(o => o.UserId == userId && CompletedStatuses.Contains(o.Status))
            .SelectMany(o => o.Items);

        var familiesSupported = await items.Select(i => i.Product.ProducerId).Distinct().CountAsync(cancellationToken);
        var distinctDistricts = await items.Select(i => i.Product.DistrictId).Distinct().CountAsync(cancellationToken);
        var distinctCategories = await items.Select(i => i.Product.CategoryId).Distinct().CountAsync(cancellationToken);
        var totalItems = await items.SumAsync(i => (int?)i.Quantity, cancellationToken) ?? 0;

        return new ImpactStatsDto
        {
            FamiliesSupported = familiesSupported,
            DistinctDistrictsSupported = distinctDistricts,
            DistinctCategoriesSupported = distinctCategories,
            TotalItemsPurchased = totalItems,
        };
    }
}
