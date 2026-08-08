using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMentorService
{
    Task<MentorProfileDto> BecomeMentorAsync(Guid userId, BecomeMentorRequest request, CancellationToken cancellationToken);
    Task<MentorProfileDto> UpdateProfileAsync(Guid userId, UpdateMentorProfileRequest request, CancellationToken cancellationToken);
    Task<MentorProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<MentorProfileDto> GetByIdAsync(Guid mentorId, CancellationToken cancellationToken);
    Task<PagedResult<MentorListItemDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}
