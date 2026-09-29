using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>
/// Service-to-service feed for the review vector index, mirroring <see cref="ProductIndexController"/>. Consumed
/// by a Python sync worker that reads pending documents and reports results back over HTTP; it never gets
/// database credentials. Reuses the same internal-key auth as the product index feed (same trust boundary),
/// but its own route and its own state table, so review vectors stay isolated from product search data.
/// </summary>
[ApiController]
[AllowAnonymous]
[InternalApiKey]
[Route("api/internal/review-index")]
public class ReviewIndexController : ControllerBase
{
    private readonly IReviewIndexService _index;

    public ReviewIndexController(IReviewIndexService index)
    {
        _index = index;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ReviewIndexBatchDto>> Pending([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        => Ok(await _index.GetPendingAsync(limit, cancellationToken));

    [HttpPost("ack")]
    public async Task<IActionResult> Ack(ReviewIndexAckRequest request, CancellationToken cancellationToken)
    {
        await _index.AckAsync(request, cancellationToken);
        return NoContent();
    }
}
