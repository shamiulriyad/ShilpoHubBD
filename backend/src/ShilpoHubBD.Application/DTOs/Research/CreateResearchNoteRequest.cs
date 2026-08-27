namespace ShilpoHubBD.Application.DTOs.Research;

public class CreateResearchNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Visibility { get; set; }
}
