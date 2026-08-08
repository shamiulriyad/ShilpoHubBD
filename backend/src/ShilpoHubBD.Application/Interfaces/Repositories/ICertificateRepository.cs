using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ICertificateRepository
{
    Task<Certificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Certificate?> GetByCertificateNumberAsync(string certificateNumber, CancellationToken cancellationToken);
    Task<Certificate?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<List<Certificate>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken);
    Task AddAsync(Certificate certificate, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
