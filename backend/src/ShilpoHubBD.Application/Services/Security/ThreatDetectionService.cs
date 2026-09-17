using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Services.Security;

public class ThreatDetectionService : IThreatDetectionService
{
    private const int SuspiciousLookbackHours = 1;
    private const int SuspiciousMinFailedAttempts = 5;

    private readonly IThreatDetectionRepository _repository;
    private readonly IUserRepository _userRepository;

    public ThreatDetectionService(IThreatDetectionRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<LoginAttemptDto>> GetFailedLoginsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _repository.GetFailedLoginsPagedAsync(page, pageSize, cancellationToken);
        return new PagedResult<LoginAttemptDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<List<SuspiciousIpDto>> GetSuspiciousIpsAsync(CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddHours(-SuspiciousLookbackHours);
        var rows = await _repository.GetSuspiciousIpsAsync(since, SuspiciousMinFailedAttempts, cancellationToken);
        return rows.Select(r => new SuspiciousIpDto { IpAddress = r.IpAddress, FailedAttempts = r.FailedCount }).ToList();
    }

    public async Task<List<BlockedIpDto>> GetBlockedIpsAsync(CancellationToken cancellationToken)
    {
        var blocked = await _repository.GetBlockedIpsAsync(cancellationToken);
        return blocked.Select(ToDto).ToList();
    }

    public async Task<BlockedIpDto> BlockIpAsync(Guid userId, BlockIpRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var ip = request.IpAddress.Trim();
        if (await _repository.GetBlockedIpByAddressAsync(ip, cancellationToken) is not null)
        {
            throw new ConflictException($"'{ip}' is already blocked.");
        }

        var blocked = new BlockedIpAddress
        {
            Id = Guid.NewGuid(),
            IpAddress = ip,
            Reason = request.Reason.Trim(),
            BlockedByUserId = userId,
            BlockedBy = user,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddBlockedIpAsync(blocked, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(blocked);
    }

    public async Task UnblockIpAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var blocked = await _repository.GetBlockedIpByAddressAsync(ipAddress.Trim(), cancellationToken)
            ?? throw new NotFoundException("That IP address is not currently blocked.");

        _repository.RemoveBlockedIp(blocked);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static LoginAttemptDto ToDto(LoginAttempt attempt) => new()
    {
        Id = attempt.Id,
        Email = attempt.Email,
        IpAddress = attempt.IpAddress,
        Succeeded = attempt.Succeeded,
        CreatedAt = attempt.CreatedAt,
    };

    private static BlockedIpDto ToDto(BlockedIpAddress blocked) => new()
    {
        Id = blocked.Id,
        IpAddress = blocked.IpAddress,
        Reason = blocked.Reason,
        BlockedByName = blocked.BlockedBy.FullName,
        ExpiresAt = blocked.ExpiresAt,
        CreatedAt = blocked.CreatedAt,
    };
}
