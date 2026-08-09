using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.BusinessPartner;

public class BusinessPartnerPreferredCategory
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerProfileId { get; set; }
    public BusinessPartnerProfile BusinessPartnerProfile { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
