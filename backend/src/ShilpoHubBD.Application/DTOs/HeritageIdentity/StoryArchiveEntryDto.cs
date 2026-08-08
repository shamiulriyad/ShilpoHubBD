namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class StoryArchiveEntryDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? Year { get; set; }
    public int DisplayOrder { get; set; }
}
