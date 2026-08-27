namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchPublicationQueryParameters
{
    public string? Type { get; set; }
    public string? Search { get; set; }
    public int? Year { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
