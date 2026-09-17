namespace ShilpoHubBD.Application.DTOs.Cms;

public class UpdateNewsItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Source { get; set; }
    public bool IsPublished { get; set; }
}
