namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class HeritageAwardDto
{
    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}
