using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISustainabilityRepository
{
    Task<SustainabilityProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SustainabilityProfile?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken);
    Task<SustainableMaterialCertification?> GetCertificationByIdAsync(Guid certificationId, CancellationToken cancellationToken);
    Task AddAsync(SustainabilityProfile profile, CancellationToken cancellationToken);
    Task AddMaterialRecordAsync(SustainableMaterialRecord record, CancellationToken cancellationToken);
    Task AddCertificationAsync(SustainableMaterialCertification certification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
