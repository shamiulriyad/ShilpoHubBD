namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class ImportKnowledgeNodeRequest
{
    public string NodeType { get; set; } = string.Empty;
    public Guid ExternalEntityId { get; set; }
    public string? LabelOverride { get; set; }
    public string? Description { get; set; }
}
