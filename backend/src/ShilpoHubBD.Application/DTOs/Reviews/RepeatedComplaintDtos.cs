using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.DTOs.Reviews;

/// <summary>A candidate historical review returned by the similarity search (RAG or DB fallback), before it's
/// hydrated with its own text/complaint/severity from Postgres.</summary>
public class SimilarReviewMatchDto
{
    public Guid ReviewId { get; set; }
    public double Score { get; set; }
}

/// <summary>A historical review actually shown to the AI comparison step — always hydrated from Postgres, never
/// from the vector search's own copy of the text, so Gemini can never be handed an invented review.</summary>
public class HistoricalReviewSnippet
{
    public Guid ReviewId { get; set; }
    public string Text { get; set; } = string.Empty;
    public ReviewComplaintType ComplaintType { get; set; }
    public ReviewSeverity Severity { get; set; }
    public bool SameProduct { get; set; }
}

public class RepeatedComplaintContext
{
    public string ProductName { get; set; } = string.Empty;
    public string NewReviewText { get; set; } = string.Empty;
    public ReviewComplaintType NewComplaintType { get; set; }
    public ReviewSeverity NewSeverity { get; set; }
    public List<HistoricalReviewSnippet> HistoricalReviews { get; set; } = new();
}

public class RepeatedComplaintResultDto
{
    public bool IsRepeatedComplaint { get; set; }
    public ReviewComplaintType ComplaintType { get; set; }
    public ReviewSeverity Severity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool IsAiGenerated { get; set; }
}
