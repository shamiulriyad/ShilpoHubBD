using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>
/// Metadata for one training experiment run and its evaluation metrics. Status and metrics are
/// recorded manually; the backend does not execute training.
/// </summary>
public class TrainingRun
{
    public Guid Id { get; set; }

    public Guid InnovationExperimentId { get; set; }
    public InnovationExperiment Experiment { get; set; } = null!;

    public Guid? ExperimentVersionId { get; set; }
    public InnovationExperimentVersion? ExperimentVersion { get; set; }

    public int RunNumber { get; set; }
    public TrainingRunStatus Status { get; set; } = TrainingRunStatus.Pending;

    public string? DatasetSnapshotName { get; set; }
    public string? HyperparametersJson { get; set; }
    public string? MetricsJson { get; set; }
    public string? PrimaryMetricName { get; set; }
    public double? PrimaryMetricValue { get; set; }
    public string? Notes { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid TriggeredByUserId { get; set; }
    public User TriggeredBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
