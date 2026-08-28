namespace ShilpoHubBD.Application.Options;

// Configurable, rule-based weights for the Eco Score and Green Production Badge (no AI/ML involved).
// Bound from the "SustainabilityScore" configuration section so weights can be tuned without a code change.
public class SustainabilityScoreOptions
{
    public int PointsPerRecycledMaterial { get; set; } = 5;
    public int PointsPerRenewableMaterial { get; set; } = 5;
    public int PointsPerLocallySourcedMaterial { get; set; } = 3;
    public int PointsPerBiodegradableMaterial { get; set; } = 4;
    public int PointsPerVerifiedCertification { get; set; } = 10;
    public decimal MaxEcoScore { get; set; } = 100;

    public int BronzeBadgeThreshold { get; set; } = 30;
    public int SilverBadgeThreshold { get; set; } = 60;
    public int GoldBadgeThreshold { get; set; } = 85;
}
