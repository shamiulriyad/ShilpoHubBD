namespace ShilpoHubBD.Application.DTOs.ArVr;

public class MuseumItemQueryParameters
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public Guid? DistrictId { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
