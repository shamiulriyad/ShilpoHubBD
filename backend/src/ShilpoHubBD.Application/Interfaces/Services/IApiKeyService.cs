using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IApiKeyService
{
    Task<CreateApiKeyResultDto> CreateAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ApiKeyDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ApiKeyDto> RevokeAsync(Guid id, CancellationToken cancellationToken);
}
