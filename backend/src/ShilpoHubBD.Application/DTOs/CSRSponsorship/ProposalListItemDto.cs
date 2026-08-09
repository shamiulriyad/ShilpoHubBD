using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class ProposalListItemDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public string BusinessPartnerName { get; set; } = string.Empty;
    public decimal FundingAmount { get; set; }
    public SponsorshipProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
