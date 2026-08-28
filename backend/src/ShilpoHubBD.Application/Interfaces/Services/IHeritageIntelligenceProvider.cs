using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Turns a bag of live platform signals into an explainable heritage-intelligence score. The default
/// implementation is rule-based; the abstraction leaves room for a future ML model without touching
/// the service or controller.
/// </summary>
public interface IHeritageIntelligenceProvider
{
    string ProviderName { get; }

    HeritageIndexComputation Compute(
        HeritageIndexType type,
        HeritageIndexScope scope,
        string scopeLabel,
        HeritageIntelligenceSignals signals);
}

/// <summary>Live signals gathered for one scope + period, handed to the provider.</summary>
public record HeritageIntelligenceSignals
{
    public int TotalProducers { get; init; }
    public int ActiveProducers { get; init; }
    public int VerifiedHeritageProducers { get; init; }
    public int CraftPractitioners { get; init; }

    public int ActiveProducts { get; init; }
    public int Orders { get; init; }
    public decimal SalesValue { get; init; }

    public int Villages { get; init; }
    public int ActiveVillages { get; init; }

    public int CulturalEvents { get; init; }
    public int HeritageFestivals { get; init; }
    public int StoryArchiveEntries { get; init; }

    public int RiskLow { get; init; }
    public int RiskModerate { get; init; }
    public int RiskHigh { get; init; }
    public int RiskCritical { get; init; }
    public int RiskSafeguarded { get; init; }
    public int ClimateRiskRecords { get; init; }
    public int MaterialScarcityRecords { get; init; }
    public int AffectedArtisans { get; init; }

    public int ApprenticeEnrollments { get; init; }
    public int ProgramApplications { get; init; }
    public int AcademyLearners { get; init; }
    public int MentorshipRequests { get; init; }
    public int CourseEnrollments { get; init; }
}

public record HeritageIndexComputation(
    decimal Score,
    HeritageIndexRating Rating,
    string Summary,
    string Method,
    IReadOnlyList<HeritageIndexComponentResult> Components);

public record HeritageIndexComponentResult(
    string Key,
    string Label,
    decimal RawValue,
    decimal Weight,
    decimal ContributionScore,
    string? Detail);
