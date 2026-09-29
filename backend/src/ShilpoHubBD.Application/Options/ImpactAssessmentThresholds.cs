namespace ShilpoHubBD.Application.Options;

// Configurable thresholds for classifying a Producer's before/after change following a Government/NGO
// intervention (rule-based only, no AI/ML). Bound from the "ImpactAssessment" configuration section so
// thresholds can be tuned without a code change. Both values are positive magnitudes: a metric whose
// percentage change (in its own "improvement direction" — see ArtisanSupportImpactService) is >=
// ImprovedThresholdPercentage is Improved; <= -DeclinedThresholdPercentage is Declined; otherwise
// NoSignificantChange.
public class ImpactAssessmentThresholds
{
    public decimal ImprovedThresholdPercentage { get; set; } = 10m;
    public decimal DeclinedThresholdPercentage { get; set; } = 10m;
}
