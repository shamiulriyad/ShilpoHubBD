using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class VerifyHeritageIdentityRequest
{
    public HeritageVerificationStatus Status { get; set; }
    public string? Notes { get; set; }
}
