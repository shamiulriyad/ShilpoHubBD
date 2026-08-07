namespace ShilpoHubBD.Application.DTOs.Community;

public class DiscussionQueryParameters
{
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
