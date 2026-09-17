namespace ShilpoHubBD.Application.DTOs.StoryGenerator;

public class GenerateCraftStoryRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string CraftType { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;
    public string? VillageOrDistrict { get; set; }
    public List<string> Materials { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
}
