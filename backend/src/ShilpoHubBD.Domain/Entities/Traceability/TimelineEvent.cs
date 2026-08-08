namespace ShilpoHubBD.Domain.Entities.Traceability;

public class TimelineEvent
{
    public Guid Id { get; set; }

    public Guid ProductTraceabilityId { get; set; }
    public ProductTraceability ProductTraceability { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime EventDate { get; set; }
    public int DisplayOrder { get; set; }
}
