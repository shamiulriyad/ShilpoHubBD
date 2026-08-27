namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchActivityDto
{
    public Guid Id { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
