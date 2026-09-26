namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class ImportKnowledgeNodeRequest
{
    public string NodeType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
}
