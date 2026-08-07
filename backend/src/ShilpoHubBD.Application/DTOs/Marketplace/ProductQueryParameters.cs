using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class ProductQueryParameters
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? DistrictId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductSortOption SortBy { get; set; } = ProductSortOption.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
