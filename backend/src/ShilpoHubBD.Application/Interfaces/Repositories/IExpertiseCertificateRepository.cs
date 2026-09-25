using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public record ProducerRatingStat(Guid ProducerId, string ProducerName, int Count, double Average);

public interface IExpertiseCertificateRepository
{
    // Customer ratings of producers: the producer rating when a customer gave one, otherwise their product rating.
    Task<List<ProducerRatingStat>> GetRatingStatsAsync(Guid? producerId, CancellationToken cancellationToken);
    Task<List<ExpertiseCertificate>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken);
    Task<List<ExpertiseCertificate>> GetAllActiveAsync(CancellationToken cancellationToken);
    Task<(string? Expertise, bool Approved)> GetProfileAsync(Guid producerId, CancellationToken cancellationToken);
    Task<bool> NumberExistsAsync(string number, CancellationToken cancellationToken);
    Task AddAsync(ExpertiseCertificate certificate, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
