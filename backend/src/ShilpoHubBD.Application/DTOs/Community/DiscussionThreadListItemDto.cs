namespace ShilpoHubBD.Application.DTOs.Community;

public class DiscussionThreadListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int ReplyCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
