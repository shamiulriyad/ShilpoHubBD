using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Product types and materials: public reads (so producer forms and filters can list them), SuperAdmin writes.
[ApiController]
[Route("api/product-types")]
public class ProductTypesController : ControllerBase
{
    private readonly IProductLookupService _service;

    public ProductTypesController(IProductLookupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _service.GetTypesAsync(includeInactive && User.IsInRole(RoleNames.SuperAdmin), cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create(SaveLookupItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreateTypeAsync(request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateTypeAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteTypeAsync(id, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/materials")]
public class MaterialsController : ControllerBase
{
    private readonly IProductLookupService _service;

    public MaterialsController(IProductLookupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupItemDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _service.GetMaterialsAsync(includeInactive && User.IsInRole(RoleNames.SuperAdmin), cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<LookupItemDto>> Create(SaveLookupItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreateMaterialAsync(request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LookupItemDto>> Update(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateMaterialAsync(id, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteMaterialAsync(id, cancellationToken);
        return NoContent();
    }
}

// The producer's own view of a product's descriptive attributes, plus review of AI suggestions.
// AI output only ever becomes final through Confirm, with the values the producer reviewed.
[ApiController]
[Route("api/products/{productId:guid}/attributes")]
[Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
public class ProductAttributesController : ControllerBase
{
    private readonly IProductAttributesService _service;

    public ProductAttributesController(IProductAttributesService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<ProductAttributesDto>> Get(Guid productId, CancellationToken cancellationToken)
        => Ok(await _service.GetAsync(productId, CurrentUserId, IsAdmin, cancellationToken));

    [HttpPut]
    public async Task<ActionResult<ProductAttributesDto>> Save(Guid productId, SaveProductAttributesRequest request, CancellationToken cancellationToken)
        => Ok(await _service.SaveAsync(productId, request, CurrentUserId, IsAdmin, cancellationToken));

    [HttpGet("suggestion")]
    public async Task<ActionResult<AttributeSuggestionDto>> GetSuggestion(Guid productId, CancellationToken cancellationToken)
    {
        var suggestion = await _service.GetPendingSuggestionAsync(productId, CurrentUserId, IsAdmin, cancellationToken);
        return suggestion is null ? NoContent() : Ok(suggestion);
    }

    // Each call may cost an AI request, so it uses the strict per-IP "auth" limiter (10 per minute).
    [HttpPost("suggestion/generate")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    public async Task<ActionResult<AttributeSuggestionDto>> GenerateSuggestion(Guid productId, CancellationToken cancellationToken)
        => Ok(await _service.GenerateSuggestionAsync(productId, CurrentUserId, IsAdmin, cancellationToken));

    [HttpPost("suggestion/{suggestionId:guid}/confirm")]
    public async Task<ActionResult<ProductAttributesDto>> Confirm(Guid productId, Guid suggestionId, ConfirmAttributeSuggestionRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ConfirmSuggestionAsync(productId, suggestionId, request, CurrentUserId, IsAdmin, cancellationToken));

    [HttpPost("suggestion/{suggestionId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid productId, Guid suggestionId, CancellationToken cancellationToken)
    {
        await _service.DismissSuggestionAsync(productId, suggestionId, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}

/// <summary>Service-to-service auth for the product index worker: a shared secret in <c>X-Internal-Key</c> (config <c>ProductIndex:ApiKey</c>).</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InternalApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public const string Header = "X-Internal-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expected = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ProductIndex:ApiKey"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new ObjectResult(new { message = "Product index API key is not configured." }) { StatusCode = StatusCodes.Status503ServiceUnavailable };
            return;
        }

        var supplied = context.HttpContext.Request.Headers[Header].ToString();
        var ok = CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(supplied)), SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
        if (!ok)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}

// Consumed by rag/products (the Python sync worker). It reads documents and reports results over HTTP;
// it never gets database credentials.
[ApiController]
[AllowAnonymous]
[InternalApiKey]
[Route("api/internal/product-index")]
public class ProductIndexController : ControllerBase
{
    private readonly IProductIndexService _index;
    private readonly IProductAttributesService _attributes;

    public ProductIndexController(IProductIndexService index, IProductAttributesService attributes)
    {
        _index = index;
        _attributes = attributes;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ProductIndexBatchDto>> Pending([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        => Ok(await _index.GetPendingAsync(limit, cancellationToken));

    [HttpPost("ack")]
    public async Task<IActionResult> Ack(ProductIndexAckRequest request, CancellationToken cancellationToken)
    {
        await _index.AckAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<Dictionary<string, int>>> Stats(CancellationToken cancellationToken)
        => Ok(await _index.GetStatsAsync(cancellationToken));

    /// <summary>Mark every product for re-indexing (after a model change or a rebuild).</summary>
    [HttpPost("requeue-all")]
    public async Task<ActionResult<object>> RequeueAll(CancellationToken cancellationToken)
        => Ok(new { requeued = await _index.RequeueAllAsync(cancellationToken) });

    /// <summary>AI proposes attributes; they stay pending until the producer confirms.</summary>
    [HttpPost("suggestions")]
    public async Task<ActionResult<AttributeSuggestionDto>> SubmitSuggestion(SubmitAttributeSuggestionRequest request, CancellationToken cancellationToken)
        => Ok(await _attributes.SubmitSuggestionAsync(request, cancellationToken));
}

// AI-assisted product search: "Jamdani khuje dao", "10,000 takar moddhe ekta bhalo Jamdani dao", "Dhakar moddhe available craft products ki ache?"
// Public, like the marketplace itself. Rate-limited with the shared "read" policy because each request may call the AI service.
[ApiController]
[AllowAnonymous]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("read")]
[Route("api/product-search")]
public class ProductSearchController : ControllerBase
{
    private readonly IProductSearchService _service;

    public ProductSearchController(IProductSearchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ProductSearchResultDto>> Search([FromQuery] ProductSearchQuery query, CancellationToken cancellationToken)
        => Ok(await _service.SearchAsync(query, cancellationToken));
}
