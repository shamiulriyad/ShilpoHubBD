using ShilpoHubBD.Application.DTOs.Sustainability;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISustainabilityService
{
    Task<SustainabilityProfileDto> GetMyProfileAsync(Guid producerId, CancellationToken cancellationToken);
    Task<SustainabilityProfileDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<SustainableMaterialRecordDto> AddMaterialRecordAsync(
        Guid producerId, CreateMaterialRecordRequest request, CancellationToken cancellationToken);
    Task<SustainableMaterialCertificationDto> AddCertificationAsync(
        Guid producerId, CreateMaterialCertificationRequest request, CancellationToken cancellationToken);
    Task<SustainableMaterialCertificationDto> VerifyCertificationAsync(Guid certificationId, CancellationToken cancellationToken);
}
