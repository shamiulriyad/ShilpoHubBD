using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.KnowledgeGraph;

/// <summary>A typed edge between two <see cref="KnowledgeNode"/>s.</summary>
public class KnowledgeRelationship
{
    public Guid Id { get; set; }

    public Guid SourceNodeId { get; set; }
    public KnowledgeNode SourceNode { get; set; } = null!;

    public Guid TargetNodeId { get; set; }
    public KnowledgeNode TargetNode { get; set; } = null!;

    public KnowledgeRelationshipType RelationshipType { get; set; }

    /// <summary>When false the edge is treated as symmetric during traversal.</summary>
    public bool IsDirected { get; set; } = true;

    public double? Weight { get; set; }
    public string? Label { get; set; }
    public string? Note { get; set; }
    public string? MetadataJson { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
