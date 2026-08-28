using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IComplaintService
{
    Task<ComplaintDto> CreateAsync(
        Guid userId, CreateComplaintRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ComplaintListItemDto>> GetPagedAsync(
        ComplaintQueryParameters query, CancellationToken cancellationToken);

    Task<ComplaintDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ComplaintDto> UpdateAsync(
        Guid userId, Guid id, UpdateComplaintRequest request, CancellationToken cancellationToken);

    Task<ComplaintDto> AddUpdateAsync(
        Guid userId, Guid id, AddComplaintUpdateRequest request, CancellationToken cancellationToken);

    Task<ComplaintDto> AssignAsync(
        Guid userId, Guid id, AssignComplaintRequest request, CancellationToken cancellationToken);

    Task<ComplaintDto> ResolveAsync(
        Guid userId, Guid id, ResolveComplaintRequest request, CancellationToken cancellationToken);

    Task<ComplaintDto> LinkFlagAsync(
        Guid userId, Guid id, LinkComplaintFlagRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
