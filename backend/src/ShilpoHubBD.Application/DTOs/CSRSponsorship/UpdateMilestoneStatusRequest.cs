using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class UpdateMilestoneStatusRequest
{
    public SponsorshipMilestoneStatus Status { get; set; }
}
