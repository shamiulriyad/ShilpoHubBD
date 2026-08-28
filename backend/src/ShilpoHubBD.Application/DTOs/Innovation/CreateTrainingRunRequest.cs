namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateTrainingRunRequest
{
    public Guid? ExperimentVersionId { get; set; }
    public string? DatasetSnapshotName { get; set; }
    public string? HyperparametersJson { get; set; }
    public string? Notes { get; set; }
}
