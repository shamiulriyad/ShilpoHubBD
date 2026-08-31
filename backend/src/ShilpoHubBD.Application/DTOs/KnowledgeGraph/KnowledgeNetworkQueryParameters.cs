namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeNetworkQueryParameters
{
    public Guid? FocusNodeId { get; set; }
    public int Depth { get; set; } = 2;
    public int MaxNodes { get; set; } = 250;
}
