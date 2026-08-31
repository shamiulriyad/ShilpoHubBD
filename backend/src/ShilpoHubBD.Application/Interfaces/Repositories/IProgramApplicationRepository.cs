using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProgramApplicationRepository
{
    Task<ProgramApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ProgramApplication>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken);
    Task<List<ProgramApplication>> GetByProgramAsync(Guid programId, CancellationToken cancellationToken);
    Task<bool> HasOpenApplicationAsync(Guid programId, Guid applicantUserId, CancellationToken cancellationToken);
    Task AddAsync(ProgramApplication application, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
