namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class UpdateHeritageRiskRecordRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string? CraftName { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? VillageId { get; set; }
    public Guid? ProducerId { get; set; }
    public int? AffectedArtisanCount { get; set; }
    public string? ContributingFactors { get; set; }
    public string? RecommendedActions { get; set; }
    public string? Source { get; set; }
    public int? AssessmentYear { get; set; }
    public DateTime? AssessedOn { get; set; }
}
