using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>An entry in a <see cref="FundingApplication"/>'s audit trail.</summary>
public class FundingApplicationEvent
{
    public Guid Id { get; set; }

    public Guid FundingApplicationId { get; set; }
    public FundingApplication Application { get; set; } = null!;

    public FundingApplicationEventType Type { get; set; }
    public string? Note { get; set; }

    public FundingApplicationStatus? FromStatus { get; set; }
    public FundingApplicationStatus? ToStatus { get; set; }

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
