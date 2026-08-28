namespace ShilpoHubBD.Application.DTOs.SupplierMatching;

public class SupplierMatchRequest
{
    public Guid? CategoryId { get; set; }
    public string? ProductKeyword { get; set; }
    public int? Quantity { get; set; }
    public decimal? MaxBudgetPerUnit { get; set; }
    public Guid? DistrictId { get; set; }
    public string? Material { get; set; }
    public bool? CertificationRequired { get; set; }
    public int? MaxDeliveryDays { get; set; }
    public decimal? MinRating { get; set; }
    public int MaxResults { get; set; } = 10;
}
