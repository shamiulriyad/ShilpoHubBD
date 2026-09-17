namespace ShilpoHubBD.Application.DTOs.Cms;

public class NewsItemQueryParameters
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
