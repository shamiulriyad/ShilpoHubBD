namespace ShilpoHubBD.Application.DTOs.Cms;

public class NewsItemListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Source { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
