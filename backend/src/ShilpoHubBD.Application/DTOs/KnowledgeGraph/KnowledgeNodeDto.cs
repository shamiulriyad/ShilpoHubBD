namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeNodeDto
{
    public Guid Id { get; set; }
    public string NodeType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Guid? ExternalEntityId { get; set; }
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsCurated { get; set; }
    public int OutgoingCount { get; set; }
    public int IncomingCount { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
