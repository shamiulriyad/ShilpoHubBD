using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Profiles;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfileDto> SetPhotoAsync(Guid userId, string? photoUrl, CancellationToken cancellationToken);
    Task<UserProfileDto> UpsertMineAsync(Guid userId, UpsertUserProfileRequest request, CancellationToken cancellationToken);

    Task<PagedResult<UserProfileListItemDto>> GetForAdminAsync(UserProfileQueryParameters query, CancellationToken cancellationToken);
    Task<UserProfileListItemDto> ApproveAsync(Guid profileId, Guid adminUserId, ReviewUserProfileRequest request, CancellationToken cancellationToken);
    Task<UserProfileListItemDto> RejectAsync(Guid profileId, Guid adminUserId, ReviewUserProfileRequest request, CancellationToken cancellationToken);
}
