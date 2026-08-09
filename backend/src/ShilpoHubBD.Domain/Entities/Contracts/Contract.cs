using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Contracts;

public class Contract
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AutoRenew { get; set; }
    public int? RenewalTermMonths { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.PendingApproval;

    // Self-reference: when a contract is renewed, a new Contract row is created linking back here
    // rather than mutating dates in place, so the full renewal chain stays auditable.
    public Guid? PreviousContractId { get; set; }
    public Contract? PreviousContract { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ContractItem> Items { get; set; } = new List<ContractItem>();
    public ICollection<ContractDeliverySchedule> DeliverySchedules { get; set; } = new List<ContractDeliverySchedule>();
    public ICollection<ContractDocument> Documents { get; set; } = new List<ContractDocument>();
    public ICollection<ContractStatusEvent> StatusHistory { get; set; } = new List<ContractStatusEvent>();
}
