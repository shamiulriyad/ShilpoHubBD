using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.DTOs.BusinessPartner;

public class VerifyBusinessPartnerRequest
{
    public BusinessVerificationStatus Status { get; set; }
    public string? Notes { get; set; }
}
