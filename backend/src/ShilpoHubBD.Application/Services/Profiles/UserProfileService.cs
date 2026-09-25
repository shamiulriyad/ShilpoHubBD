using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Services.Profiles;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserProfileRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return ToDto(user, profile);
    }

    public async Task<UserProfileDto> UpsertMineAsync(Guid userId, UpsertUserProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var nid = request.NidNumber.Trim();
        var expertise = string.IsNullOrWhiteSpace(request.Expertise) ? null : request.Expertise.Trim();

        if (IsProducer(user) && expertise is null)
        {
            throw new ConflictException("Producers must state their expertise (for example Jamdani weaving or pottery).");
        }

        if (request.DistrictId.HasValue && !await _repository.DistrictExistsAsync(request.DistrictId.Value, cancellationToken))
        {
            throw new ConflictException("District not found.");
        }

        if (await _repository.NidInUseAsync(nid, userId, cancellationToken))
        {
            throw new ConflictException("This NID number is already registered to another account.");
        }

        var now = DateTime.UtcNow;
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken);
        var isNew = profile is null;
        profile ??= new UserProfile { Id = Guid.NewGuid(), UserId = userId, CreatedAt = now };

        profile.LegalName = request.LegalName.Trim();
        profile.Phone = request.Phone.Trim();
        profile.NidNumber = nid;
        profile.Expertise = expertise;
        profile.DistrictId = request.DistrictId;
        profile.AddressLine = request.AddressLine.Trim();
        profile.About = string.IsNullOrWhiteSpace(request.About) ? null : request.About.Trim();
        profile.UpdatedAt = now;

        // Any change goes back to the admin for another look.
        profile.Status = UserProfileStatus.Pending;
        profile.ReviewedByUserId = null;
        profile.ReviewedAt = null;
        profile.ReviewNotes = null;

        if (isNew)
        {
            await _repository.AddAsync(profile, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(user, await _repository.GetByUserIdAsync(userId, cancellationToken));
    }

    public async Task<PagedResult<UserProfileListItemDto>> GetForAdminAsync(UserProfileQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 50 ? 20 : query.PageSize;

        var (items, total) = await _repository.GetPagedAsync(query, cancellationToken);
        var roles = await _repository.GetRolesAsync(items.Select(i => i.UserId), cancellationToken);

        return new PagedResult<UserProfileListItemDto>
        {
            Items = items.Select(p => ToListItem(p, roles.TryGetValue(p.UserId, out var r) ? r : new List<string>())).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public Task<UserProfileListItemDto> ApproveAsync(Guid profileId, Guid adminUserId, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => ReviewAsync(profileId, adminUserId, UserProfileStatus.Approved, request, cancellationToken);

    public Task<UserProfileListItemDto> RejectAsync(Guid profileId, Guid adminUserId, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => ReviewAsync(profileId, adminUserId, UserProfileStatus.Rejected, request, cancellationToken);

    private async Task<UserProfileListItemDto> ReviewAsync(
        Guid profileId, Guid adminUserId, UserProfileStatus target, ReviewUserProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        if (profile.Status != UserProfileStatus.Pending)
        {
            throw new ConflictException("Only a profile that is waiting for review can be approved or rejected.");
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (target == UserProfileStatus.Rejected && notes is null)
        {
            throw new ConflictException("Tell the member why the profile was rejected so they can fix it.");
        }

        profile.Status = target;
        profile.ReviewedByUserId = adminUserId;
        profile.ReviewedAt = DateTime.UtcNow;
        profile.ReviewNotes = notes;
        profile.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        var roles = await _repository.GetRolesAsync(new[] { profile.UserId }, cancellationToken);
        return ToListItem(profile, roles.TryGetValue(profile.UserId, out var r) ? r : new List<string>());
    }

    private static bool IsProducer(User user) => user.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer);

    private static UserProfileDto ToDto(User user, UserProfile? profile) => new()
    {
        Id = profile?.Id,
        LoginEmail = user.Email,
        Exists = profile is not null,
        LegalName = profile?.LegalName ?? string.Empty,
        Phone = profile?.Phone ?? string.Empty,
        NidNumber = profile?.NidNumber ?? string.Empty,
        Expertise = profile?.Expertise,
        DistrictId = profile?.DistrictId,
        DistrictName = profile?.District?.Name,
        AddressLine = profile?.AddressLine ?? string.Empty,
        About = profile?.About,
        Status = profile?.Status.ToString() ?? "NotSubmitted",
        ReviewNotes = profile?.ReviewNotes,
        ReviewedAt = profile?.ReviewedAt,
        ExpertiseRequired = IsProducer(user),
    };

    private static UserProfileListItemDto ToListItem(UserProfile p, List<string> roles) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        LoginName = p.User.FullName,
        LoginEmail = p.User.Email,
        Roles = roles,
        LegalName = p.LegalName,
        Phone = p.Phone,
        NidNumber = p.NidNumber,
        Expertise = p.Expertise,
        DistrictName = p.District?.Name,
        AddressLine = p.AddressLine,
        Status = p.Status.ToString(),
        ReviewNotes = p.ReviewNotes,
        UpdatedAt = p.UpdatedAt,
    };
}
