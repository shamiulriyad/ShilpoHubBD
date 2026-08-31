using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Portfolio?> GetByAcademyMemberProfileIdAsync(Guid academyMemberProfileId, CancellationToken cancellationToken);
    Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken);

    Task<PortfolioProject?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddProjectAsync(PortfolioProject project, CancellationToken cancellationToken);
    void RemoveProject(PortfolioProject project);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
