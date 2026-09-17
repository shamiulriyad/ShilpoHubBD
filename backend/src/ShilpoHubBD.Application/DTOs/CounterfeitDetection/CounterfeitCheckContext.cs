using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.CounterfeitDetection;

public class CounterfeitCheckContext
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CategoryAveragePrice { get; set; }
    public int CategorySampleSize { get; set; }
    public HandmadeVerificationStatus HandmadeVerificationStatus { get; set; }
    public ProductApprovalStatus ApprovalStatus { get; set; }
    public int ReviewCount { get; set; }
    public int SalesCount { get; set; }
}
