namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CulturalStoryChapterDto
{
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public int DisplayOrder { get; set; }
}
