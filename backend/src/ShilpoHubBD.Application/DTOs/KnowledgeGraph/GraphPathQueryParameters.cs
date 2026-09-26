namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class GraphPathQueryParameters
{
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public int MaxDepth { get; set; } = 50;
    public string? RelationshipTypes { get; set; }
}
