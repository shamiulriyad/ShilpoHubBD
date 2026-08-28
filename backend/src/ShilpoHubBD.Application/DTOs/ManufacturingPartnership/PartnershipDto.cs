using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

public class PartnershipDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string ProductRequirements { get; set; } = string.Empty;
    public string ManufacturingSpecifications { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }

    public DateTime TimelineStartDate { get; set; }
    public DateTime TimelineEndDate { get; set; }

    public PartnershipStatus Status { get; set; }
    public string? ProducerResponseNotes { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int ProgressPercentage { get; set; }

    public List<MilestoneDto> Milestones { get; set; } = new();
    public List<PartnershipStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
