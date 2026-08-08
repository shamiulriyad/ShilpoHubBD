using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISearchService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}
