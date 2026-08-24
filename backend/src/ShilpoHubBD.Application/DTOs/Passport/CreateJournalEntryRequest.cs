namespace ShilpoHubBD.Application.DTOs.Passport;

public class CreateJournalEntryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public Guid? CheckInId { get; set; }
}
