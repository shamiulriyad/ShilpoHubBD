using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<JobApplication>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken);
    Task<List<JobApplication>> GetByJobListingAsync(Guid jobListingId, CancellationToken cancellationToken);
    Task<bool> HasOpenApplicationAsync(Guid jobListingId, Guid applicantUserId, CancellationToken cancellationToken);
    Task AddAsync(JobApplication application, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
