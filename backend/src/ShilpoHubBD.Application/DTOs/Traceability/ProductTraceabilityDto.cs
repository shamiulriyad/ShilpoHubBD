namespace ShilpoHubBD.Application.DTOs.Traceability;

public class ProductTraceabilityDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<MaterialSourceDto> MaterialSources { get; set; } = new();
    public List<TimelineEventDto> TimelineEvents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
