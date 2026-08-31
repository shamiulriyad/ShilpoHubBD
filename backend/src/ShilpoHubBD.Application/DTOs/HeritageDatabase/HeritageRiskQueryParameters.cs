namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageRiskQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Level { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? VillageId { get; set; }
    public int? AssessmentYear { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
