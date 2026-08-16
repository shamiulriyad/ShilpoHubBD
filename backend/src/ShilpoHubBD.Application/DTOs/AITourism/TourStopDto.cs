namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourStopDto
{
    public Guid? ReferenceId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
