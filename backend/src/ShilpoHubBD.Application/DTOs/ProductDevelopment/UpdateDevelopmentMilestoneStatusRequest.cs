using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class UpdateDevelopmentMilestoneStatusRequest
{
    public DevelopmentMilestoneStatus Status { get; set; }
}
