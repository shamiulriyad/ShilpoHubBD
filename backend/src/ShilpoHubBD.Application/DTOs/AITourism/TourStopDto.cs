namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TourStopDto
{
    public Guid? ReferenceId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    // Visit length in hours. Taken from the dataset when it states one, otherwise a rough estimate
    // -- DurationIsEstimated says which, so the UI never presents a guess as a verified figure.
    public double? EstimatedDurationHours { get; set; }
    public bool DurationIsEstimated { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
