namespace ShilpoHubBD.Application.DTOs.Passport;

public class UpdateJournalEntryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
