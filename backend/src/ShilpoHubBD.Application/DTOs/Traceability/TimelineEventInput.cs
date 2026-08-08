namespace ShilpoHubBD.Application.DTOs.Traceability;

public class TimelineEventInput
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime EventDate { get; set; }
}
