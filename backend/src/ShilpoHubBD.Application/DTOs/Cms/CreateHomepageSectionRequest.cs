namespace ShilpoHubBD.Application.DTOs.Cms;

public class CreateHomepageSectionRequest
{
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
}
