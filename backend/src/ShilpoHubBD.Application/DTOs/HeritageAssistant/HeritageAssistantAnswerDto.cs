namespace ShilpoHubBD.Application.DTOs.HeritageAssistant;

public class HeritageAssistantAnswerDto
{
    public string Answer { get; set; } = string.Empty;
    public List<string> RelatedDistricts { get; set; } = new();
    public List<string> RelatedFestivals { get; set; } = new();
    public List<string> RelatedUnescoRecords { get; set; } = new();

    // Populated by RagHeritageAssistantProvider from the RAG service's response; left empty by
    // RuleBasedHeritageAssistantProvider. Sources is the dataset file(s) an answer was drawn from
    // (e.g. "craftDetails.json"), Category is the detected question type (e.g. "craft_location").
    public List<string> Sources { get; set; } = new();
    public string? Category { get; set; }

    /// <summary>In-site pages the answer was drawn from (e.g. performing-arts detail pages).</summary>
    public List<HeritageAssistantLinkDto> Links { get; set; } = new();
}

public class HeritageAssistantLinkDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
