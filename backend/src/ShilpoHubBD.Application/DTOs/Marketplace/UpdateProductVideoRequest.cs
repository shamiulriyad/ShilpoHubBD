namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class UpdateProductVideoRequest
{
    public string VideoUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
}
