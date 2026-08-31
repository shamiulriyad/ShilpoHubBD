using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ITrainingCertificateRepository
{
    Task<TrainingCertificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TrainingCertificate?> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken cancellationToken);
    Task<TrainingCertificate?> GetByApprenticeEnrollmentIdAsync(Guid apprenticeEnrollmentId, CancellationToken cancellationToken);
    Task<TrainingCertificate?> GetActiveSkillCertificateAsync(Guid recipientUserId, Guid heritageSkillId, CancellationToken cancellationToken);
    Task<TrainingCertificate?> GetByCertificateNumberAsync(string certificateNumber, CancellationToken cancellationToken);
    Task<List<TrainingCertificate>> GetByRecipientAsync(Guid recipientUserId, CancellationToken cancellationToken);
    Task AddAsync(TrainingCertificate certificate, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
