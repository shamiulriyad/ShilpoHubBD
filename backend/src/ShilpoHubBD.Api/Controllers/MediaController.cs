using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
public class MediaController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private readonly IWebHostEnvironment _environment;

    public MediaController(IWebHostEnvironment environment) => _environment = environment;

    [HttpPost("images")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
    public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > 20 * 1024 * 1024)
            return BadRequest(new { message = "Choose an image smaller than 20 MB." });
        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPG, PNG and WebP images are supported." });

        var extension = file.ContentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "images");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new { url = $"/uploads/images/{name}" });
    }
}
