using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchPublicationService
{
    Task<PagedResult<ResearchPublicationDto>> BrowseAsync(
        Guid userId, ResearchPublicationQueryParameters query, CancellationToken cancellationToken);

    Task<ResearchPublicationDto> GetByIdAsync(Guid userId, Guid publicationId, CancellationToken cancellationToken);

    Task<List<ResearchPublicationDto>> GetForProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchPublicationDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchPublicationRequest request, CancellationToken cancellationToken);

    Task<ResearchPublicationDto> UpdateAsync(
        Guid userId, Guid projectId, Guid publicationId, UpdateResearchPublicationRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid publicationId, CancellationToken cancellationToken);
}
