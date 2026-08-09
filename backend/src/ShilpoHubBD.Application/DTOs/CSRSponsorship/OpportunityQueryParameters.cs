using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class OpportunityQueryParameters
{
    public SponsorshipOpportunityStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
