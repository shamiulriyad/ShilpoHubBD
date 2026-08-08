namespace ShilpoHubBD.Application.DTOs.Traceability;

public class CreateProductTraceabilityRequest
{
    public Guid ProductId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<MaterialSourceInput> MaterialSources { get; set; } = new();
    public List<TimelineEventInput> TimelineEvents { get; set; } = new();
}
