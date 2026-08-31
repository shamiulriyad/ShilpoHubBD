using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/ar-vr/craft-scans")]
public class ArCraftScanController : ControllerBase
{
    private readonly IArCraftScanService _arCraftScanService;

    public ArCraftScanController(IArCraftScanService arCraftScanService)
    {
        _arCraftScanService = arCraftScanService;
    }

    private Guid? CurrentUserIdOrNull
    {
        get
        {
            var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    [HttpPost]
    public async Task<ActionResult<ArCraftScanResultDto>> Scan(ArCraftScanRequest request, CancellationToken cancellationToken)
    {
        var result = await _arCraftScanService.ScanAsync(CurrentUserIdOrNull, request, cancellationToken);
        return Ok(result);
    }
}
