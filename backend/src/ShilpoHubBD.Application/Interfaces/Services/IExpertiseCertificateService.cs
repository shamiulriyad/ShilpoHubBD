using ShilpoHubBD.Application.DTOs.Certificates;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IExpertiseCertificateService
{
    Task<ExpertiseProgressDto> GetMineAsync(Guid producerId, CancellationToken cancellationToken);
    Task<List<EligibleProducerDto>> GetEligibleAsync(CancellationToken cancellationToken);
    Task<ExpertiseCertificateDto> IssueAsync(Guid producerId, Guid adminUserId, CancellationToken cancellationToken);
}
