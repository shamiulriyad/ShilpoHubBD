using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Real-world profile (name, expertise, location, phone, NID) of the signed-in member, plus the admin review of it.
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserProfileService _service;
    private readonly ShilpoHubBD.Application.Interfaces.Repositories.IUserProfileRepository _repository;

    public ProfileController(IUserProfileService service, ShilpoHubBD.Application.Interfaces.Repositories.IUserProfileRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Expertise values of approved producers, for the "filter producers by expertise" dropdowns.
    [AllowAnonymous]
    [HttpGet("api/profile/expertise-options")]
    public async Task<ActionResult<List<string>>> ExpertiseOptions(CancellationToken cancellationToken)
        => Ok(await _repository.GetProducerExpertiseOptionsAsync(cancellationToken));

    [HttpGet("api/profile/me")]
    public async Task<ActionResult<UserProfileDto>> GetMine(CancellationToken cancellationToken)
        => Ok(await _service.GetMineAsync(CurrentUserId, cancellationToken));

    private static readonly HashSet<string> PhotoTypes = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
    private const string AvatarUrlPrefix = "/uploads/avatars/";
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    // Any signed-in member can add, change or remove their own profile photo.
    [HttpPost("api/profile/photo")]
    [RequestSizeLimit(MaxPhotoBytes + 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxPhotoBytes + 1024)]
    public async Task<ActionResult<UserProfileDto>> UploadPhoto(IFormFile file, [FromServices] IWebHostEnvironment environment, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > MaxPhotoBytes)
            return BadRequest(new { message = "Choose an image smaller than 5 MB." });
        if (!PhotoTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPG, PNG and WebP images are supported." });

        var extension = file.ContentType.ToLowerInvariant() switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "avatars");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(folder, name)))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var previous = (await _service.GetMineAsync(CurrentUserId, cancellationToken)).PhotoUrl;
        var result = await _service.SetPhotoAsync(CurrentUserId, AvatarUrlPrefix + name, cancellationToken);
        DeleteAvatarFile(environment, previous);
        return Ok(result);
    }

    [HttpDelete("api/profile/photo")]
    public async Task<ActionResult<UserProfileDto>> RemovePhoto([FromServices] IWebHostEnvironment environment, CancellationToken cancellationToken)
    {
        var previous = (await _service.GetMineAsync(CurrentUserId, cancellationToken)).PhotoUrl;
        var result = await _service.SetPhotoAsync(CurrentUserId, null, cancellationToken);
        DeleteAvatarFile(environment, previous);
        return Ok(result);
    }

    // Only ever deletes files this feature wrote (uploads/avatars/<guid>.<ext>).
    private static void DeleteAvatarFile(IWebHostEnvironment environment, string? url)
    {
        if (url is null || !url.StartsWith(AvatarUrlPrefix, StringComparison.Ordinal)) return;
        var fileName = Path.GetFileName(url);
        var path = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "avatars", fileName);
        try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch (IOException) { /* stale file is harmless */ }
    }

    [HttpPut("api/profile/me")]
    public async Task<ActionResult<UserProfileDto>> UpsertMine(UpsertUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpsertMineAsync(CurrentUserId, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("api/admin/profiles")]
    public async Task<ActionResult<PagedResult<UserProfileListItemDto>>> GetForAdmin(
        [FromQuery] UserProfileQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetForAdminAsync(query, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("api/admin/profiles/{id:guid}/approve")]
    public async Task<ActionResult<UserProfileListItemDto>> Approve(Guid id, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ApproveAsync(id, CurrentUserId, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("api/admin/profiles/{id:guid}/reject")]
    public async Task<ActionResult<UserProfileListItemDto>> Reject(Guid id, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(id, CurrentUserId, request, cancellationToken));
}
