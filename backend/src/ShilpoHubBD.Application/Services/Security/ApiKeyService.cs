using System.Security.Cryptography;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Application.Services.Security;

public class ApiKeyService : IApiKeyService
{
    private const string KeyPrefixTag = "shb_";

    private readonly IApiKeyRepository _repository;
    private readonly IUserRepository _userRepository;

    public ApiKeyService(IApiKeyRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<CreateApiKeyResultDto> CreateAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var rawSecret = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var rawKey = $"{KeyPrefixTag}{rawSecret}";
        var keyHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey)));
        var displayPrefix = rawKey[..(KeyPrefixTag.Length + 8)];

        var now = DateTime.UtcNow;
        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            KeyPrefix = displayPrefix,
            KeyHash = keyHash,
            CreatedByUserId = userId,
            CreatedBy = user,
            IsActive = true,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = now,
        };

        await _repository.AddAsync(key, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResultDto
        {
            Id = key.Id,
            Name = key.Name,
            ApiKey = rawKey,
            KeyPrefix = key.KeyPrefix,
            ExpiresAt = key.ExpiresAt,
            CreatedAt = key.CreatedAt,
        };
    }

    public async Task<PagedResult<ApiKeyDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, cancellationToken);
        return new PagedResult<ApiKeyDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ApiKeyDto> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var key = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("API key not found.");

        if (!key.IsActive)
        {
            throw new ConflictException("API key is already revoked.");
        }

        key.IsActive = false;
        key.RevokedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(key);
    }

    private static ApiKeyDto ToDto(ApiKey key) => new()
    {
        Id = key.Id,
        Name = key.Name,
        KeyPrefix = key.KeyPrefix,
        CreatedByName = key.CreatedBy.FullName,
        IsActive = key.IsActive,
        LastUsedAt = key.LastUsedAt,
        ExpiresAt = key.ExpiresAt,
        RevokedAt = key.RevokedAt,
        CreatedAt = key.CreatedAt,
    };
}
