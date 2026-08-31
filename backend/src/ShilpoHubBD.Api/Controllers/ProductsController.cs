using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/products")]
[EnableRateLimiting("read")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetProducts(
        [FromQuery] ProductQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetFeatured(
        CancellationToken cancellationToken, [FromQuery] int count = 8)
    {
        var result = await _productService.GetFeaturedAsync(count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("trending")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetTrending(
        CancellationToken cancellationToken, [FromQuery] int count = 8)
    {
        var result = await _productService.GetTrendingAsync(count, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("mine")]
    public async Task<ActionResult<List<ProductDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _productService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _productService.GetBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPatch("{id:guid}/featured")]
    public async Task<ActionResult<ProductDto>> SetFeatured(Guid id, SetFeaturedRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.SetFeaturedAsync(id, request.IsFeatured, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{productId:guid}/variants")]
    public async Task<ActionResult<ProductDto>> AddVariant(Guid productId, CreateProductVariantRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.AddVariantAsync(productId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateVariant(Guid productId, Guid variantId, UpdateProductVariantRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateVariantAsync(productId, variantId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    public async Task<ActionResult<ProductDto>> DeleteVariant(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteVariantAsync(productId, variantId, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkCreateProductsResultDto>> BulkCreate(BulkCreateProductsRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.BulkCreateAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{productId:guid}/videos")]
    public async Task<ActionResult<ProductDto>> AddVideo(Guid productId, CreateProductVideoRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.AddVideoAsync(productId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{productId:guid}/videos/{videoId:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateVideo(Guid productId, Guid videoId, UpdateProductVideoRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateVideoAsync(productId, videoId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{productId:guid}/videos/{videoId:guid}")]
    public async Task<ActionResult<ProductDto>> DeleteVideo(Guid productId, Guid videoId, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteVideoAsync(productId, videoId, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPatch("{id:guid}/handmade-verification")]
    public async Task<ActionResult<ProductDto>> SetHandmadeVerification(Guid id, SetHandmadeVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.SetHandmadeVerificationAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}
