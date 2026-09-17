using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class SetProductApprovalRequest
{
    public ProductApprovalStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
