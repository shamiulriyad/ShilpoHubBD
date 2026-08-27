namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class LiveHeritageQueryParameters
{
    public string? Search { get; set; }
    public Guid? DistrictId { get; set; }
    public string? Division { get; set; }
    public string? Craft { get; set; }
    public bool IncludeInactive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
