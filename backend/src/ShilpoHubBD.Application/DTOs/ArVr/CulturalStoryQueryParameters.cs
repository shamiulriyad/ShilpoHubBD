namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CulturalStoryQueryParameters
{
    public Guid? HeritagePlaceId { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
