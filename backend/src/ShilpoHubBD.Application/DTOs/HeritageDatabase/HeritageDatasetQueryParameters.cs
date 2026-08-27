namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? AccessLevel { get; set; }
    public string? Tag { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
