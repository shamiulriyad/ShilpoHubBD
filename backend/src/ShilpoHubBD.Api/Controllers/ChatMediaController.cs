using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShilpoHubBD.Api.Controllers;

// Picture uploads any signed-in member can attach to a chat message or a product question.
[ApiController]
[Route("api/media")]
[Authorize]
public class ChatMediaController : ControllerBase
{
    private const int MaxBytes = 8 * 1024 * 1024;
    private readonly IWebHostEnvironment _environment;

    public ChatMediaController(IWebHostEnvironment environment) => _environment = environment;

    [HttpPost("chat-images")]
    [RequestSizeLimit(MaxBytes + 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes + 1024)]
    public async Task<ActionResult<object>> UploadChatImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > MaxBytes)
            return BadRequest(new { message = "Choose an image smaller than 8 MB." });

        // Decide the type from the file's own first bytes, not from what the client claims.
        var header = new byte[12];
        await using (var probe = file.OpenReadStream())
        {
            var read = await probe.ReadAsync(header, cancellationToken);
            if (read < 12) return BadRequest(new { message = "That does not look like an image." });
        }

        string? extension = null;
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) extension = ".jpg";
        else if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) extension = ".png";
        else if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) extension = ".webp";
        if (extension is null)
            return BadRequest(new { message = "Only JPG, PNG and WebP pictures are supported." });

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "chat");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new { url = $"/uploads/chat/{name}" });
    }
}
