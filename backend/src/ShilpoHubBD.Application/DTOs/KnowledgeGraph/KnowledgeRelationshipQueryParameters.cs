namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeRelationshipQueryParameters
{
    public string? RelationshipType { get; set; }
    public Guid? NodeId { get; set; }
    public Guid? SourceNodeId { get; set; }
    public Guid? TargetNodeId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
