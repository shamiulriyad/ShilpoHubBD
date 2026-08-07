namespace ShilpoHubBD.Application.DTOs.Community;

public class CreateDiscussionThreadRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
