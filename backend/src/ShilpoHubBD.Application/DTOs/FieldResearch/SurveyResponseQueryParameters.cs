namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyResponseQueryParameters
{
    public string? Status { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public bool MineOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
