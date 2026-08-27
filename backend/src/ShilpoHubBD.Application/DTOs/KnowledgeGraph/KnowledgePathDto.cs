using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgePathDto
{
    public bool Found { get; set; }
    public int Length { get; set; }
    public List<KnowledgeNodeDto> Nodes { get; set; } = new();
    public List<KnowledgeRelationshipDto> Relationships { get; set; } = new();
}
