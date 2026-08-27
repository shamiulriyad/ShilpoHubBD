namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class GraphTraversalQueryParameters
{
    public int Depth { get; set; } = 2;
    public string? RelationshipTypes { get; set; }
    public string Direction { get; set; } = "both";
    public int MaxNodes { get; set; } = 250;
}
