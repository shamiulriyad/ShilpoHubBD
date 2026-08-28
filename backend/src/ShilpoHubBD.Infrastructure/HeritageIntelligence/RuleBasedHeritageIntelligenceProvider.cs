using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Infrastructure.HeritageIntelligence;

/// <summary>
/// Rule-based stand-in for a future ML model behind the Government / NGO Heritage Intelligence
/// indices. Every score is a transparent weighted blend of saturating sub-scores over the signal bag
/// it is handed -- no external calls, no model weights. Swap for a real
/// <see cref="IHeritageIntelligenceProvider"/> later without touching the service or controller.
/// </summary>
public class RuleBasedHeritageIntelligenceProvider : IHeritageIntelligenceProvider
{
    public const string Name = "rule-based-heritage-intel-v1";

    public string ProviderName => Name;

    public HeritageIndexComputation Compute(
        HeritageIndexType type,
        HeritageIndexScope scope,
        string scopeLabel,
        HeritageIntelligenceSignals s)
    {
        return type switch
        {
            HeritageIndexType.HeritageRiskIndex => Risk(s, scopeLabel),
            HeritageIndexType.LivingHeritageIndex => LivingHeritage(s, scopeLabel),
            HeritageIndexType.CraftHealthScore => CraftHealth(s, scopeLabel),
            HeritageIndexType.VillageSurvivalIndex => VillageSurvival(s, scopeLabel),
            HeritageIndexType.YouthParticipation => YouthParticipation(s, scopeLabel),
            HeritageIndexType.ClimateRiskAnalysis => ClimateRisk(s, scopeLabel),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown heritage index type."),
        };
    }

    // ---- Risk direction: higher score = MORE endangered -----------------

    private HeritageIndexComputation Risk(HeritageIntelligenceSignals s, string label)
    {
        var weightedRisk = (s.RiskLow * 1) + (s.RiskModerate * 2) + (s.RiskHigh * 4)
            + (s.RiskCritical * 7) - (s.RiskSafeguarded * 3);
        var pressure = Saturate(Math.Max(0, weightedRisk), 12);
        var artisanExposure = Saturate(s.AffectedArtisans, 400);
        var marketThinness = 100 - Saturate(s.ActiveProducts + s.Orders, 60);
        var successorGap = 100 - Saturate(s.ApprenticeEnrollments + s.ProgramApplications, 15);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("risk_pressure", "Weighted risk assessments", weightedRisk, 0.45m, pressure,
                $"{s.RiskCritical} critical / {s.RiskHigh} high / {s.RiskModerate} moderate / {s.RiskLow} low, {s.RiskSafeguarded} safeguarded"),
            Comp("affected_artisans", "Artisans flagged as affected", s.AffectedArtisans, 0.20m, artisanExposure, null),
            Comp("market_thinness", "Thin market activity", s.ActiveProducts + s.Orders, 0.20m, marketThinness,
                $"{s.ActiveProducts} active products, {s.Orders} orders"),
            Comp("successor_gap", "Few incoming learners", s.ApprenticeEnrollments + s.ProgramApplications, 0.15m, successorGap, null),
        };

        var score = WeightedScore(components);
        return Build(score, label, RiskDirectionRating(score),
            $"Heritage Risk Index for {label}: {Round(score)}/100 (higher means more endangered). "
            + $"Driven mainly by {DominantComponent(components)}.",
            components);
    }

    private HeritageIndexComputation ClimateRisk(HeritageIntelligenceSignals s, string label)
    {
        var climate = Saturate(s.ClimateRiskRecords, 5);
        var material = Saturate(s.MaterialScarcityRecords, 5);
        var severity = Saturate((s.RiskHigh * 2) + (s.RiskCritical * 4), 16);
        var resilience = 100 - Saturate(s.RiskSafeguarded + s.VerifiedHeritageProducers, 20);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("climate_records", "Climate-threat assessments", s.ClimateRiskRecords, 0.45m, climate, null),
            Comp("material_scarcity", "Raw-material scarcity assessments", s.MaterialScarcityRecords, 0.25m, material, null),
            Comp("risk_severity", "High/critical risk load", (s.RiskHigh * 2) + (s.RiskCritical * 4), 0.20m, severity, null),
            Comp("low_resilience", "Limited safeguarding capacity", s.RiskSafeguarded + s.VerifiedHeritageProducers, 0.10m, resilience, null),
        };

        var score = WeightedScore(components);
        return Build(score, label, RiskDirectionRating(score),
            $"Climate Risk Analysis for {label}: {Round(score)}/100 (higher means more climate-exposed). "
            + $"{s.ClimateRiskRecords} climate and {s.MaterialScarcityRecords} material-scarcity assessments on record.",
            components);
    }

    // ---- Health direction: higher score = HEALTHIER -------------------

    private HeritageIndexComputation LivingHeritage(HeritageIntelligenceSignals s, string label)
    {
        var artisanBase = Saturate(s.ActiveProducers, 40);
        var authenticity = Saturate(s.VerifiedHeritageProducers, 25);
        var transmission = Saturate(s.ApprenticeEnrollments + s.CourseEnrollments + s.MentorshipRequests, 30);
        var expression = Saturate(s.CulturalEvents + s.HeritageFestivals + s.StoryArchiveEntries, 20);
        var livelihood = Saturate(s.ActiveProducts + s.Orders, 80);
        var riskDrag = Saturate((s.RiskHigh * 2) + (s.RiskCritical * 4), 14);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("active_artisans", "Active producers", s.ActiveProducers, 0.28m, artisanBase, null),
            Comp("authenticity", "Verified heritage identities", s.VerifiedHeritageProducers, 0.18m, authenticity, null),
            Comp("transmission", "Learners in training / mentorship", s.ApprenticeEnrollments + s.CourseEnrollments + s.MentorshipRequests, 0.22m, transmission, null),
            Comp("cultural_expression", "Events, festivals & recorded stories", s.CulturalEvents + s.HeritageFestivals + s.StoryArchiveEntries, 0.14m, expression, null),
            Comp("livelihood", "Marketplace activity", s.ActiveProducts + s.Orders, 0.18m, livelihood, null),
            Comp("risk_drag", "High/critical risk load (penalty)", (s.RiskHigh * 2) + (s.RiskCritical * 4), 0.10m, 100 - riskDrag, null),
        };

        var score = WeightedScore(components);
        return Build(score, label, HealthDirectionRating(score),
            $"Living Heritage Index for {label}: {Round(score)}/100 (higher is healthier). "
            + $"{s.ActiveProducers} active producers, {s.VerifiedHeritageProducers} verified, "
            + $"{s.ApprenticeEnrollments + s.CourseEnrollments} in training.",
            components);
    }

    private HeritageIndexComputation CraftHealth(HeritageIntelligenceSignals s, string label)
    {
        var practitioners = Saturate(s.CraftPractitioners, 30);
        var market = Saturate(s.ActiveProducts + s.Orders, 50);
        var revenue = Saturate((double)s.SalesValue, 500_000);
        var pipeline = Saturate(s.ApprenticeEnrollments + s.ProgramApplications, 20);
        var riskDrag = Saturate((s.RiskHigh * 3) + (s.RiskCritical * 6) + s.RiskModerate, 18);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("practitioners", "Producers practising the craft", s.CraftPractitioners, 0.30m, practitioners, null),
            Comp("market_activity", "Products & orders", s.ActiveProducts + s.Orders, 0.22m, market, null),
            Comp("revenue", "Sales value", s.SalesValue, 0.18m, revenue, null),
            Comp("succession_pipeline", "Learners entering the craft", s.ApprenticeEnrollments + s.ProgramApplications, 0.18m, pipeline, null),
            Comp("risk_drag", "Risk assessments (penalty)", (s.RiskHigh * 3) + (s.RiskCritical * 6) + s.RiskModerate, 0.12m, 100 - riskDrag, null),
        };

        var score = WeightedScore(components);
        return Build(score, label, HealthDirectionRating(score),
            $"Craft Health Score for {label}: {Round(score)}/100 (higher is healthier). "
            + $"{s.CraftPractitioners} practitioners, {s.ActiveProducts} products listed.",
            components);
    }

    private HeritageIndexComputation VillageSurvival(HeritageIntelligenceSignals s, string label)
    {
        var artisanBase = Saturate(s.ActiveProducers, 25);
        var craftContinuity = Saturate(s.CraftPractitioners, 15);
        var economy = Saturate(s.ActiveProducts + s.Orders, 40);
        var youth = Saturate(s.ApprenticeEnrollments + s.ProgramApplications + s.MentorshipRequests, 18);
        var displacementDrag = Saturate((s.RiskHigh * 2) + (s.RiskCritical * 5) + s.AffectedArtisans / 20, 20);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("artisan_base", "Active producers in the area", s.ActiveProducers, 0.28m, artisanBase, null),
            Comp("craft_continuity", "Producers on the village craft", s.CraftPractitioners, 0.18m, craftContinuity, null),
            Comp("local_economy", "Products & orders", s.ActiveProducts + s.Orders, 0.22m, economy, null),
            Comp("youth_pipeline", "Learners & mentees", s.ApprenticeEnrollments + s.ProgramApplications + s.MentorshipRequests, 0.20m, youth, null),
            Comp("displacement_drag", "Risk & affected artisans (penalty)", (s.RiskHigh * 2) + (s.RiskCritical * 5), 0.12m, 100 - displacementDrag, null),
        };

        var score = WeightedScore(components);
        return Build(score, label, HealthDirectionRating(score),
            $"Village Survival Index for {label}: {Round(score)}/100 (higher means more likely to sustain its craft tradition).",
            components);
    }

    private HeritageIndexComputation YouthParticipation(HeritageIntelligenceSignals s, string label)
    {
        var apprentices = Saturate(s.ApprenticeEnrollments, 30);
        var applications = Saturate(s.ProgramApplications, 40);
        var academy = Saturate(s.AcademyLearners + s.CourseEnrollments, 60);
        var mentorship = Saturate(s.MentorshipRequests, 25);
        var relativeToBase = s.ActiveProducers == 0
            ? 0
            : Saturate((double)(s.ApprenticeEnrollments + s.CourseEnrollments) / s.ActiveProducers * 100, 60);

        var components = new List<HeritageIndexComponentResult>
        {
            Comp("apprentice_enrollments", "Apprentice enrolments", s.ApprenticeEnrollments, 0.28m, apprentices, null),
            Comp("program_applications", "Programme applications", s.ProgramApplications, 0.18m, applications, null),
            Comp("academy_learners", "Academy learners & course enrolments", s.AcademyLearners + s.CourseEnrollments, 0.24m, academy, null),
            Comp("mentorship", "Mentorship requests", s.MentorshipRequests, 0.15m, mentorship, null),
            Comp("learners_per_producer", "Learners relative to active producers", s.ActiveProducers, 0.15m, relativeToBase,
                $"{s.ApprenticeEnrollments + s.CourseEnrollments} learners / {s.ActiveProducers} producers"),
        };

        var score = WeightedScore(components);
        return Build(score, label, HealthDirectionRating(score),
            $"Youth Participation for {label}: {Round(score)}/100 (higher is better). "
            + $"{s.ApprenticeEnrollments} apprentices, {s.AcademyLearners + s.CourseEnrollments} academy learners.",
            components);
    }

    // ---- helpers -----------------------------------------------------

    private static HeritageIndexComputation Build(
        decimal score, string label, HeritageIndexRating rating, string summary,
        List<HeritageIndexComponentResult> components)
        => new(score, rating, summary, Name, components);

    private static HeritageIndexComponentResult Comp(
        string key, string labelText, decimal raw, decimal weight, decimal subScore, string? detail)
        => new(key, labelText, raw, weight, Math.Round(weight * subScore, 2), detail);

    private static decimal WeightedScore(IReadOnlyList<HeritageIndexComponentResult> components)
    {
        var totalWeight = components.Sum(c => c.Weight);
        if (totalWeight == 0)
        {
            return 0;
        }

        var raw = components.Sum(c => c.ContributionScore) / totalWeight;
        return Math.Clamp(Math.Round(raw, 2), 0, 100);
    }

    /// <summary>Saturating normaliser: 0 → 0, x = k → 50, x → ∞ → 100.</summary>
    private static decimal Saturate(double x, double k)
    {
        if (x <= 0 || k <= 0)
        {
            return 0;
        }

        return (decimal)Math.Round(100.0 * x / (x + k), 2);
    }

    private static decimal Saturate(int x, double k) => Saturate((double)x, k);

    private static int Round(decimal score) => (int)Math.Round(score, MidpointRounding.AwayFromZero);

    private static string DominantComponent(IReadOnlyList<HeritageIndexComponentResult> components)
        => components.OrderByDescending(c => c.ContributionScore).First().Label.ToLowerInvariant();

    private static HeritageIndexRating HealthDirectionRating(decimal score) => score switch
    {
        >= 80 => HeritageIndexRating.Strong,
        >= 60 => HeritageIndexRating.Good,
        >= 40 => HeritageIndexRating.Fair,
        >= 20 => HeritageIndexRating.Poor,
        _ => HeritageIndexRating.Critical,
    };

    private static HeritageIndexRating RiskDirectionRating(decimal score) => score switch
    {
        >= 80 => HeritageIndexRating.Critical,
        >= 60 => HeritageIndexRating.Poor,
        >= 40 => HeritageIndexRating.Fair,
        >= 20 => HeritageIndexRating.Good,
        _ => HeritageIndexRating.Strong,
    };
}
