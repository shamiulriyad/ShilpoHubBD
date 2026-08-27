namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class FieldEvidenceQueryParameters
{
    public string? EvidenceType { get; set; }
    public Guid? SurveyResponseId { get; set; }
    public bool MineOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
