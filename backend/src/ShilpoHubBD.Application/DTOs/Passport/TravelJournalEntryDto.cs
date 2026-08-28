namespace ShilpoHubBD.Application.DTOs.Passport;

public class TravelJournalEntryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public string? HeritagePlaceName { get; set; }
    public Guid? CheckInId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
