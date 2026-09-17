namespace ShilpoHubBD.Application.DTOs.Cms;

public class BlogPostQueryParameters
{
    public string? Search { get; set; }
    public string? Tag { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
