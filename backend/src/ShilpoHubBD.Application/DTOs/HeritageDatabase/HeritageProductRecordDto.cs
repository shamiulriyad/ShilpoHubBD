namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageProductRecordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string HandmadeVerificationStatus { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SalesCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
