namespace ShilpoHubBD.Application.DTOs.Innovation;

public class InnovationExperimentQueryParameters
{
    public string? Status { get; set; }
    public string? ModelType { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
