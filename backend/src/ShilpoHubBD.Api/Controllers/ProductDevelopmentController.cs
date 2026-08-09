using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/product-development")]
[Authorize]
public class ProductDevelopmentController : ControllerBase
{
    private readonly IProductDevelopmentService _productDevelopmentService;

    public ProductDevelopmentController(IProductDevelopmentService productDevelopmentService)
    {
        _productDevelopmentService = productDevelopmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<DevelopmentProjectDto>> Create(CreateDevelopmentProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<DevelopmentProjectListItemDto>>> GetMine(
        [FromQuery] DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.GetForBusinessPartnerAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpGet("received")]
    public async Task<ActionResult<PagedResult<DevelopmentProjectListItemDto>>> GetReceived(
        [FromQuery] DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.GetForProducerAsync(CurrentUserId, parameters, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DevelopmentProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/respond")]
    public async Task<ActionResult<DevelopmentProjectDto>> Respond(Guid id, DevelopmentResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.RespondAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<DevelopmentCommentDto>> AddComment(Guid id, AddDevelopmentCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.AddCommentAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<DevelopmentMilestoneDto>> AddMilestone(Guid id, DevelopmentMilestoneInput request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.AddMilestoneAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/milestones/{milestoneId:guid}/status")]
    public async Task<ActionResult<DevelopmentMilestoneDto>> UpdateMilestoneStatus(
        Guid id, Guid milestoneId, UpdateDevelopmentMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.UpdateMilestoneStatusAsync(id, milestoneId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.Producer)]
    [HttpPost("{id:guid}/prototypes")]
    public async Task<ActionResult<PrototypeVersionDto>> SubmitPrototype(Guid id, SubmitPrototypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.SubmitPrototypeAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/prototypes/{prototypeVersionId:guid}/decision")]
    public async Task<ActionResult<PrototypeVersionDto>> DecidePrototype(
        Guid id, Guid prototypeVersionId, PrototypeDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.DecidePrototypeAsync(id, prototypeVersionId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/convert-to-product")]
    public async Task<ActionResult<DevelopmentProjectDto>> ConvertToProduct(Guid id, ConvertToProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.ConvertToProductAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<DevelopmentProjectDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productDevelopmentService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }
}
