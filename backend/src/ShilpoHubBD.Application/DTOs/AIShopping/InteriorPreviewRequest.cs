namespace ShilpoHubBD.Application.DTOs.AIShopping;

public class InteriorPreviewRequest
{
    public Guid ProductId { get; set; }

    // Read once by the API layer (Application has no ASP.NET Core / IFormFile dependency) and passed
    // down as plain bytes, the same pattern used for ImageStorageOptions.WebRootPath elsewhere.
    public byte[] RoomImageBytes { get; set; } = Array.Empty<byte>();
    public string RoomImageContentType { get; set; } = string.Empty;

    /// <summary>Optional shopper-supplied placement/style guidance, e.g. "against the far wall" or "modern look".</summary>
    public string? Style { get; set; }
}
