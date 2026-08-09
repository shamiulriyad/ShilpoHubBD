using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.DTOs.BusinessPartner;

public class BusinessPartnerQueryParameters
{
    public string? Search { get; set; }
    public BusinessType? BusinessType { get; set; }
    public BusinessVerificationStatus? VerificationStatus { get; set; }
    public Guid? DistrictId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
