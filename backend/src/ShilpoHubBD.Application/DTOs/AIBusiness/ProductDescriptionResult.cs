namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductDescriptionResult
{
    public string Description { get; set; } = string.Empty;
    public List<string> SuggestedTags { get; set; } = new();
}
