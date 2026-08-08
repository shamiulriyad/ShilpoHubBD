namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class ProductVideoDto
{
    public Guid Id { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
}
