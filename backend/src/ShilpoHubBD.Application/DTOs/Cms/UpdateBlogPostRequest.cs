namespace ShilpoHubBD.Application.DTOs.Cms;

public class UpdateBlogPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
}
