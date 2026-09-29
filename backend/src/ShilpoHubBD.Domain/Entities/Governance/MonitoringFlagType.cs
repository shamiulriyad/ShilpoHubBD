namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>What a <see cref="MonitoringFlag"/> is about.</summary>
public enum MonitoringFlagType
{
    FraudRisk,
    FakeProduct,
    ReviewAbuse,
    QrAnomaly,
    ComplianceGap,
    Other,

    // ---- AI Moderation (Super Admin) ----
    SpamContent,
    PolicyViolation,
    InappropriateImage,

    /// <summary>Repeated/similar customer complaints detected for one product (AI review-moderation Part 2).</summary>
    RepeatedProductComplaints,
}
