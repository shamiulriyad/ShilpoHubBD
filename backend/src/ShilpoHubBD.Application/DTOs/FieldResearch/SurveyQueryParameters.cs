namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyQueryParameters
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public string? Scope { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
