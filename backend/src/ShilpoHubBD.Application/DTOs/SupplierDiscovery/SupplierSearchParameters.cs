using ShilpoHubBD.Domain.Entities.SupplierDiscovery;

namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

public class SupplierSearchParameters
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? DistrictId { get; set; }
    public string? ProductName { get; set; }
    public string? Material { get; set; }
    public decimal? MinRating { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinProductionCapacity { get; set; }
    public bool? HandmadeVerifiedOnly { get; set; }
    public bool? CertifiedOnly { get; set; }
    public SupplierSortOption SortBy { get; set; } = SupplierSortOption.RatingDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
