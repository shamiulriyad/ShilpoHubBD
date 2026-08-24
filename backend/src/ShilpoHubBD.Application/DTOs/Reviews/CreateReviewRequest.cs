namespace ShilpoHubBD.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    // Exactly one of ProductId/HeritagePlaceId/BookingId must be set.
    public Guid? ProductId { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public Guid? BookingId { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
}
