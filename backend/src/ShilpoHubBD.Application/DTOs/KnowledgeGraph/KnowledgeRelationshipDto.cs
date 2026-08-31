namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeRelationshipDto
{
    public Guid Id { get; set; }
    public Guid SourceNodeId { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
    public string SourceNodeType { get; set; } = string.Empty;
    public Guid TargetNodeId { get; set; }
    public string TargetLabel { get; set; } = string.Empty;
    public string TargetNodeType { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public bool IsDirected { get; set; }
    public double? Weight { get; set; }
    public string? Label { get; set; }
    public string? Note { get; set; }
    public string? MetadataJson { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
