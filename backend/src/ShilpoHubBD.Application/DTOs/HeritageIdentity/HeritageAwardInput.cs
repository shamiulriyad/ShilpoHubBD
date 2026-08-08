namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class HeritageAwardInput
{
    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
