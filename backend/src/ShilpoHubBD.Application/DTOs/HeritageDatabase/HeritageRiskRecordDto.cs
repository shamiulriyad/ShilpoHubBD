namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageRiskRecordDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string? CraftName { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public Guid? VillageId { get; set; }
    public string? VillageName { get; set; }
    public Guid? ProducerId { get; set; }
    public string? ProducerName { get; set; }
    public int? AffectedArtisanCount { get; set; }
    public string? ContributingFactors { get; set; }
    public string? RecommendedActions { get; set; }
    public string? Source { get; set; }
    public int? AssessmentYear { get; set; }
    public DateTime? AssessedOn { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
