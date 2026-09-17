namespace ShilpoHubBD.Application.DTOs.HeritageAssistant;

public class HeritageAssistantAnswerDto
{
    public string Answer { get; set; } = string.Empty;
    public List<string> RelatedDistricts { get; set; } = new();
    public List<string> RelatedFestivals { get; set; } = new();
    public List<string> RelatedUnescoRecords { get; set; } = new();
}
