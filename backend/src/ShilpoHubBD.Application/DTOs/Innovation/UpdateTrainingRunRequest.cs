namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdateTrainingRunRequest
{
    public string Status { get; set; } = string.Empty;
    public string? DatasetSnapshotName { get; set; }
    public string? HyperparametersJson { get; set; }
    public string? MetricsJson { get; set; }
    public string? PrimaryMetricName { get; set; }
    public double? PrimaryMetricValue { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
