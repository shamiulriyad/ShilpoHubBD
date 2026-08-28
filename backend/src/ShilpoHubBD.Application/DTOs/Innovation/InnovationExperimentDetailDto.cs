using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class InnovationExperimentDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public string? Framework { get; set; }
    public string? ConfigJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public int VersionCount { get; set; }
    public int RunCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExperimentVersionDto> Versions { get; set; } = new();
    public List<TrainingRunDto> Runs { get; set; } = new();
}
