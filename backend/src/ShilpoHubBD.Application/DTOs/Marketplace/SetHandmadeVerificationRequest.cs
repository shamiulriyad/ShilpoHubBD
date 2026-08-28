using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class SetHandmadeVerificationRequest
{
    public HandmadeVerificationStatus Status { get; set; }
    public string? Notes { get; set; }
}
