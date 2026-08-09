using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class SponsorshipMilestoneDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public SponsorshipMilestoneStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
