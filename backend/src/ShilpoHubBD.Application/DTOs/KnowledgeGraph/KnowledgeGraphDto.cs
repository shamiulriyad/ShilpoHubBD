using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeGraphDto
{
    public List<KnowledgeNodeDto> Nodes { get; set; } = new();
    public List<KnowledgeRelationshipDto> Relationships { get; set; } = new();
    public int DepthReached { get; set; }
    public bool Truncated { get; set; }
}
