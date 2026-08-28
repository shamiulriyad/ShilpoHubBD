using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.KnowledgeGraph;

/// <summary>
/// A node in the Heritage Knowledge Graph. Flexible: it either references an existing platform entity
/// (<see cref="ExternalEntityId"/>) or stands alone as a label-only concept (Craft / Material / Culture).
/// </summary>
public class KnowledgeNode
{
    public Guid Id { get; set; }

    public KnowledgeNodeType NodeType { get; set; }

    public string Label { get; set; } = string.Empty;

    /// <summary>Lower-cased, trimmed <see cref="Label"/>; used for de-duplication within a node type.</summary>
    public string LabelNormalized { get; set; } = string.Empty;

    /// <summary>PK of the reused entity (User / Village / Product / HeritagePlace / ProducerHeritageIdentity); null for concept nodes.</summary>
    public Guid? ExternalEntityId { get; set; }

    public string? Description { get; set; }

    /// <summary>Free-form JSON metadata for the node.</summary>
    public string? MetadataJson { get; set; }

    public bool IsCurated { get; set; } = true;

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<KnowledgeRelationship> OutgoingRelationships { get; set; } = new List<KnowledgeRelationship>();
    public ICollection<KnowledgeRelationship> IncomingRelationships { get; set; } = new List<KnowledgeRelationship>();
}
