namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeRelationshipRuleDto
{
    public string SourceType { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string ReverseLabel { get; set; } = string.Empty;
}
