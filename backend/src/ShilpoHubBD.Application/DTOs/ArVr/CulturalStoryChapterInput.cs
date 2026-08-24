using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CulturalStoryChapterInput
{
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public ArVrMediaType? MediaType { get; set; }
}
