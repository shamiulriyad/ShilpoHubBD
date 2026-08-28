namespace ShilpoHubBD.Application.DTOs.ArVr;

public class ArScannedProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Story { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? MakingProcessVideoUrl { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
}
