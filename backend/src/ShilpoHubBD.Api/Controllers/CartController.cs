using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<CartItemDto>>> GetCart(CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<CartSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _cartService.GetSummaryAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> AddItem(AddToCartRequest request, CancellationToken cancellationToken)
    {
        var result = await _cartService.AddOrIncrementAsync(CurrentUserId, request.ProductId, request.ProductVariantId, request.Quantity, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{itemId:guid}")]
    public async Task<ActionResult<CartItemDto>> UpdateQuantity(Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateQuantityAsync(CurrentUserId, itemId, request.Quantity, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        await _cartService.RemoveItemAsync(CurrentUserId, itemId, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        await _cartService.ClearCartAsync(CurrentUserId, cancellationToken);
        return NoContent();
    }
}
