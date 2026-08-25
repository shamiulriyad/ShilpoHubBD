using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPortfolioService
{
    Task<PortfolioDto> GetMyPortfolioAsync(Guid userId, CancellationToken cancellationToken);

    Task<PortfolioDto> GetPublicPortfolioAsync(Guid academyMemberProfileId, CancellationToken cancellationToken);

    Task<PortfolioDto> UpdateMyPortfolioAsync(Guid userId, UpdatePortfolioRequest request, CancellationToken cancellationToken);

    Task<PortfolioDto> UpdateVisibilityAsync(Guid userId, UpdatePortfolioVisibilityRequest request, CancellationToken cancellationToken);

    Task<PortfolioProjectDto> AddProjectAsync(Guid userId, CreatePortfolioProjectRequest request, CancellationToken cancellationToken);

    Task<PortfolioProjectDto> UpdateProjectAsync(Guid userId, Guid projectId, UpdatePortfolioProjectRequest request, CancellationToken cancellationToken);

    Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);
}
