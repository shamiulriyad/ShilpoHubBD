namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeEntityCandidateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Description { get; set; }
}
