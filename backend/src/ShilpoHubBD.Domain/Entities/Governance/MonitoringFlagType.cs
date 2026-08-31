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
}
