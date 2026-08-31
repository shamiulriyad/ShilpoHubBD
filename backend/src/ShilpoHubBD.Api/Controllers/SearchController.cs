using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/search")]
[EnableRateLimiting("read")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Search(
        [FromQuery] string q, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _searchService.SearchAsync(q, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
