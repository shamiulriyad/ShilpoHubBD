namespace ShilpoHubBD.Application.DTOs.Innovation;

public class TrainingRunDto
{
    public Guid Id { get; set; }
    public Guid InnovationExperimentId { get; set; }
    public Guid? ExperimentVersionId { get; set; }
    public int? ExperimentVersionNumber { get; set; }
    public int RunNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DatasetSnapshotName { get; set; }
    public string? HyperparametersJson { get; set; }
    public string? MetricsJson { get; set; }
    public string? PrimaryMetricName { get; set; }
    public double? PrimaryMetricValue { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid TriggeredByUserId { get; set; }
    public string TriggeredByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
