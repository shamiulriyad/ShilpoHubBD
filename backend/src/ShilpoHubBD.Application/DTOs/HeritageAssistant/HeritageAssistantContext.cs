namespace ShilpoHubBD.Application.DTOs.HeritageAssistant;

public record HeritageDistrictFact(string Name, string Division);
public record HeritageFestivalFact(string Name, string Description, string DistrictName);
public record HeritageUnescoFact(string Title, string Description, int InscribedYear);

/// <summary>Pre-fetched knowledge base handed to the provider — keeps the provider a pure function.</summary>
public class HeritageAssistantContext
{
    public string Question { get; set; } = string.Empty;
    public List<HeritageDistrictFact> Districts { get; set; } = new();
    public List<HeritageFestivalFact> Festivals { get; set; } = new();
    public List<HeritageUnescoFact> UnescoRecords { get; set; } = new();
}
