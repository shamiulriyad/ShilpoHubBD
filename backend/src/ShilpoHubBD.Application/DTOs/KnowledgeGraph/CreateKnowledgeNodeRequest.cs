namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class CreateKnowledgeNodeRequest
{
    public string NodeType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Guid? ExternalEntityId { get; set; }
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
}
