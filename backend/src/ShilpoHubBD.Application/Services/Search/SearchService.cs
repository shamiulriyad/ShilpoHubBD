using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Search;

public class SearchService : ISearchService
{
    private readonly ISearchProvider _searchProvider;

    public SearchService(ISearchProvider searchProvider)
    {
        _searchProvider = searchProvider;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new PagedResult<ProductListItemDto> { Items = new(), TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var (items, totalCount) = await _searchProvider.SearchAsync(query.Trim(), page, pageSize, cancellationToken);

        return new PagedResult<ProductListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    private static ProductListItemDto ToListItemDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        Price = product.Price,
        DiscountPrice = product.DiscountPrice,
        PrimaryImageUrl = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        CategoryId = product.CategoryId,
        CategoryName = product.Category.Name,
        DistrictId = product.DistrictId,
        DistrictName = product.District.Name,
        ProducerId = product.ProducerId,
        ProducerName = product.Producer.FullName,
        AverageRating = product.AverageRating,
        ReviewCount = product.ReviewCount,
        IsFeatured = product.IsFeatured,
    };
}
