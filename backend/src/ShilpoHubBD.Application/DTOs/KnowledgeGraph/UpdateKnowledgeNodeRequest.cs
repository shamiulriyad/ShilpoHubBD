namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class UpdateKnowledgeNodeRequest
{
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsCurated { get; set; } = true;
}
