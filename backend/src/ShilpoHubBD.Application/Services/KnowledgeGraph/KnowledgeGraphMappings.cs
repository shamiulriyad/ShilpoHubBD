using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Services.KnowledgeGraph;

internal static class KnowledgeGraphMappings
{
    public static KnowledgeNodeDto ToDto(this KnowledgeNode n, int outgoing = 0, int incoming = 0) => new()
    {
        Id = n.Id,
        NodeType = n.NodeType.ToString(),
        Label = n.Label,
        ExternalEntityId = n.ExternalEntityId,
        Description = n.Description,
        MetadataJson = n.MetadataJson,
        IsCurated = n.IsCurated,
        OutgoingCount = outgoing,
        IncomingCount = incoming,
        CreatedByUserId = n.CreatedByUserId,
        CreatedByName = n.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt,
    };

    public static KnowledgeRelationshipDto ToDto(this KnowledgeRelationship r) => new()
    {
        Id = r.Id,
        SourceNodeId = r.SourceNodeId,
        SourceLabel = r.SourceNode?.Label ?? string.Empty,
        SourceNodeType = r.SourceNode?.NodeType.ToString() ?? string.Empty,
        TargetNodeId = r.TargetNodeId,
        TargetLabel = r.TargetNode?.Label ?? string.Empty,
        TargetNodeType = r.TargetNode?.NodeType.ToString() ?? string.Empty,
        RelationshipType = r.RelationshipType.ToString(),
        IsDirected = r.IsDirected,
        Weight = r.Weight,
        Label = r.Label,
        Note = r.Note,
        MetadataJson = r.MetadataJson,
        CreatedByUserId = r.CreatedByUserId,
        CreatedByName = r.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
