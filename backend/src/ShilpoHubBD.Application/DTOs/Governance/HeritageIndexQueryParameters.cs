namespace ShilpoHubBD.Application.DTOs.Governance;

public class HeritageIndexQueryParameters
{
    public string? IndexType { get; set; }
    public string? Scope { get; set; }
    public Guid? ScopeId { get; set; }
    public string? CraftLabel { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
