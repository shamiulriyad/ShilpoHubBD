using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cms/blogs")]
public class CmsBlogsController : ControllerBase
{
    private readonly IBlogPostService _service;

    public CmsBlogsController(IBlogPostService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<BlogPostListItemDto>>> GetPaged(
        [FromQuery] BlogPostQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(query, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("drafts")]
    public async Task<ActionResult<PagedResult<BlogPostListItemDto>>> GetDrafts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
        => Ok(await _service.GetDraftsAsync(page, pageSize, cancellationToken));

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<BlogPostDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        => Ok(await _service.GetBySlugAsync(slug, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BlogPostDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<BlogPostDto>> Create(CreateBlogPostRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BlogPostDto>> Update(Guid id, UpdateBlogPostRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
