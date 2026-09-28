using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

public class ProducerPartnershipStatusEvent
{
    public Guid Id { get; set; }

    public Guid AgreementId { get; set; }
    public ProducerPartnershipAgreement Agreement { get; set; } = null!;

    public ProducerPartnershipAgreementStatus Status { get; set; }
    public string? Note { get; set; }

    public Guid? ChangedByUserId { get; set; }
    public User? ChangedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
