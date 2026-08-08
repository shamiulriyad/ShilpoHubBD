using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Placeholder implementation returning mock data. No AI integration -- replace with a real
// AI-backed implementation later by registering a different IInteriorPreviewService.
public class InteriorPreviewService : IInteriorPreviewService
{
    public Task<InteriorPreviewDto> GetPreviewAsync(InteriorPreviewRequest request, CancellationToken cancellationToken)
    {
        var product = string.IsNullOrWhiteSpace(request.ProductName) ? "this product" : request.ProductName;
        var room = string.IsNullOrWhiteSpace(request.RoomType) ? "your room" : request.RoomType;

        var preview = new InteriorPreviewDto
        {
            PreviewImageUrl = "https://placehold.co/800x600?text=Interior+Preview+Coming+Soon",
            Description = $"Mock preview: {product} placed in a {room}. Photorealistic AI preview is not yet available.",
        };

        return Task.FromResult(preview);
    }
}
