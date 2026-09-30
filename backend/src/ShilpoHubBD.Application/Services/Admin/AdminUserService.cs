using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Services.Admin;

/// <summary>Super Admin user directory: search/filter users, activate or suspend accounts, and create
/// Government/NGO accounts directly (that role's self-registration is disabled).</summary>
public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _repository;
    private readonly IIdentityVerificationRepository _verificationRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogisticsPartnerRepository _logisticsPartners;

    public AdminUserService(
        IAdminUserRepository repository, IIdentityVerificationRepository verificationRepository, IAuditLogService auditLogService,
        IUserRepository userRepository, IRoleRepository roleRepository, IPasswordHasher passwordHasher,
        ILogisticsPartnerRepository logisticsPartners)
    {
        _repository = repository;
        _verificationRepository = verificationRepository;
        _auditLogService = auditLogService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _logisticsPartners = logisticsPartners;
    }

    public async Task<PagedResult<AdminUserListItemDto>> GetPagedAsync(
        AdminUserQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);
        var statuses = await _verificationRepository.GetLatestStatusesByUserIdsAsync(
            items.Select(u => u.Id), cancellationToken);

        return new PagedResult<AdminUserListItemDto>
        {
            Items = items
                .Select(u => u.ToListItemDto(statuses.TryGetValue(u.Id, out var s) ? s.ToString() : "None"))
                .ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<AdminUserDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var verifications = await _verificationRepository.GetByUserIdAsync(id, cancellationToken);
        return user.ToDetailDto(verifications.Select(v => v.ToDto()).ToList());
    }

    public async Task<AdminUserDetailDto> SetActiveAsync(
        Guid id, bool isActive, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        var actor = await _repository.GetByIdWithRolesAsync(actorUserId, cancellationToken);
        await _auditLogService.LogAsync(
            actorUserId, actor?.FullName ?? actorUserId.ToString(),
            isActive ? "AdminUser.Activated" : "AdminUser.Deactivated", "User", id,
            $"{(isActive ? "Activated" : "Deactivated")} account for {user.FullName}.", ipAddress, cancellationToken);

        var verifications = await _verificationRepository.GetByUserIdAsync(id, cancellationToken);
        return user.ToDetailDto(verifications.Select(v => v.ToDto()).ToList());
    }

    public async Task<AdminUserDetailDto> CreateGovernmentNgoUserAsync(
        CreateGovernmentNgoUserRequest request, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken)
        => await CreateManagedUserAsync(request, RoleNames.GovernmentNGO, null, actorUserId, ipAddress, cancellationToken);

    public async Task<AdminUserDetailDto> CreateLogisticsUserAsync(
        Guid profileId, CreateGovernmentNgoUserRequest request, Guid actorUserId, string? ipAddress, CancellationToken cancellationToken)
    {
        var profile = await _logisticsPartners.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Logistics company not found.");
        if (profile.UserId.HasValue) throw new ConflictException("This company already has an operator account.");
        return await CreateManagedUserAsync(request, RoleNames.LogisticsPartner, profile, actorUserId, ipAddress, cancellationToken);
    }

    private async Task<AdminUserDetailDto> CreateManagedUserAsync(
        CreateGovernmentNgoUserRequest request, string roleName,
        Domain.Entities.Logistics.LogisticsPartnerProfile? profile,
        Guid actorUserId, string? ipAddress, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken)
            ?? throw new NotFoundException($"The {roleName} role is not configured.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? email : request.FullName.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = now,
            AssignedByUserId = actorUserId,
        });

        await _userRepository.AddAsync(user, cancellationToken);
        // Both repositories share the scoped DbContext: account, role, and link commit together.
        if (profile is not null)
        {
            profile.UserId = user.Id;
            profile.User = user;
            profile.UpdatedAt = now;
        }
        await _userRepository.SaveChangesAsync(cancellationToken);

        var actor = await _repository.GetByIdWithRolesAsync(actorUserId, cancellationToken);
        await _auditLogService.LogAsync(
            actorUserId, actor?.FullName ?? actorUserId.ToString(),
            $"AdminUser.Created{roleName}", "User", user.Id,
            $"Created an admin-managed {roleName} account for {user.Email}.", ipAddress, cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    private async Task<Domain.Entities.Identity.User> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
}
