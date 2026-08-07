using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> GetWishlist(CancellationToken cancellationToken)
    {
        var result = await _wishlistService.GetWishlistAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WishlistItemDto>> Add(AddToWishlistRequest request, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.AddAsync(CurrentUserId, request.ProductId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken cancellationToken)
    {
        await _wishlistService.RemoveAsync(CurrentUserId, productId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{productId:guid}/move-to-cart")]
    public async Task<ActionResult<CartItemDto>> MoveToCart(Guid productId, MoveToCartRequest? request, CancellationToken cancellationToken)
    {
        var result = await _wishlistService.MoveToCartAsync(CurrentUserId, productId, request ?? new MoveToCartRequest(), cancellationToken);
        return Ok(result);
    }
}
