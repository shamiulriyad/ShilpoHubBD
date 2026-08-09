namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class CreateDevelopmentProjectRequest
{
    public Guid ProducerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BusinessRequirements { get; set; } = string.Empty;
    public string ProductSpecifications { get; set; } = string.Empty;
}
