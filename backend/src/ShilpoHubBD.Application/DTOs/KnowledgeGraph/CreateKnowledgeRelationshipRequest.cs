namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class CreateKnowledgeRelationshipRequest
{
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public bool IsDirected { get; set; } = true;
    public double? Weight { get; set; }
    public string? Label { get; set; }
    public string? Note { get; set; }
    public string? MetadataJson { get; set; }
}
