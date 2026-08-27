namespace ShilpoHubBD.Application.DTOs.KnowledgeGraph;

public class KnowledgeNodeQueryParameters
{
    public string? NodeType { get; set; }
    public string? Search { get; set; }
    public bool? HasExternalEntity { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
