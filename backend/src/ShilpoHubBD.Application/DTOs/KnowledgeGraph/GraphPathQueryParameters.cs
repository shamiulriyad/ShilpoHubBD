namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class GraphPathQueryParameters
{
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public int MaxDepth { get; set; } = 5;
    public string? RelationshipTypes { get; set; }
}
