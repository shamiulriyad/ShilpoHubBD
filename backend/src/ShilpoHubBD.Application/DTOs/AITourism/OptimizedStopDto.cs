namespace ShilpoHubBD.Application.DTOs.AITourism;

public class OptimizedStopDto
{
    public Guid PlaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public double DistanceFromPreviousKm { get; set; }

    /// <summary>Null when this leg fell back to straight-line distance (no real road duration exists for it).</summary>
    public double? EstimatedTravelMinutesFromPrevious { get; set; }
}
