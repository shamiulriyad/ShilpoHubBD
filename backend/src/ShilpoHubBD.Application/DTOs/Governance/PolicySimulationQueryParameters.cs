namespace ShilpoHubBD.Application.DTOs.Governance;

public class PolicySimulationQueryParameters
{
    public string? SimulationType { get; set; }
    public string? Scope { get; set; }
    public Guid? ScopeId { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
