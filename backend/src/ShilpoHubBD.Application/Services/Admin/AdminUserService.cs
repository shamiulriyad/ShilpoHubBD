using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Admin;

/// <summary>Super Admin user directory: search/filter users and activate or suspend accounts.</summary>
public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _repository;
    private readonly IIdentityVerificationRepository _verificationRepository;
    private readonly IAuditLogService _auditLogService;

    public AdminUserService(
        IAdminUserRepository repository, IIdentityVerificationRepository verificationRepository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _verificationRepository = verificationRepository;
        _auditLogService = auditLogService;
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

    private async Task<Domain.Entities.Identity.User> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");
}
