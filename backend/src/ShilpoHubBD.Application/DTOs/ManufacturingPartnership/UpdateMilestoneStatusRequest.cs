using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class UpdateMilestoneStatusRequest
{
    public MilestoneStatus Status { get; set; }
}
