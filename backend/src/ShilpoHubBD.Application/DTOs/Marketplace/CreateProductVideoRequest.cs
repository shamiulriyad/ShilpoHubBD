namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class CreateProductVideoRequest
{
    public string VideoUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
}
