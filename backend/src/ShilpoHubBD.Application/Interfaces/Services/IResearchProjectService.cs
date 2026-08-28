using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchProjectService
{
    Task<PagedResult<ResearchProjectListItemDto>> GetMineAsync(
        Guid userId, ResearchProjectQueryParameters query, CancellationToken cancellationToken);

    Task<ResearchProjectDetailDto> GetByIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchProjectDetailDto> CreateAsync(
        Guid userId, CreateResearchProjectRequest request, CancellationToken cancellationToken);

    Task<ResearchProjectDetailDto> UpdateAsync(
        Guid userId, Guid projectId, UpdateResearchProjectRequest request, CancellationToken cancellationToken);

    Task<ResearchProjectDetailDto> UpdateStatusAsync(
        Guid userId, Guid projectId, UpdateResearchProjectStatusRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<List<ResearchProjectMemberDto>> GetMembersAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchProjectMemberDto> AddMemberAsync(
        Guid userId, Guid projectId, AddResearchProjectMemberRequest request, CancellationToken cancellationToken);

    Task<ResearchProjectMemberDto> UpdateMemberRoleAsync(
        Guid userId, Guid projectId, Guid memberId, UpdateResearchMemberRoleRequest request, CancellationToken cancellationToken);

    Task RemoveMemberAsync(Guid userId, Guid projectId, Guid memberId, CancellationToken cancellationToken);

    Task<List<ResearchActivityDto>> GetActivityAsync(Guid userId, Guid projectId, int take, CancellationToken cancellationToken);
}
