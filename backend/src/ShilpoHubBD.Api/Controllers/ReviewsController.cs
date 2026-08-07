using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetByProduct(
        Guid productId, [FromQuery] ReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetByProductAsync(productId, query, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByProduct), new { productId = result.ProductId }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> Update(Guid id, UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _reviewService.UpdateAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _reviewService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}
