namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>A citation produced by the Citation Generator, optionally linked to a project publication.</summary>
public class ResearchAICitation
{
    public Guid Id { get; set; }

    public Guid ResearchAIAnalysisId { get; set; }
    public ResearchAIAnalysis Analysis { get; set; } = null!;

    public Guid? ResearchPublicationId { get; set; }
    public ResearchPublication? Publication { get; set; }

    public ResearchCitationStyle Style { get; set; }

    public string SourceTitle { get; set; } = string.Empty;
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Container { get; set; }
    public string? Doi { get; set; }
    public string? Url { get; set; }

    public string FormattedCitation { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
