namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>The kind of platform record a monitoring flag or complaint points at.</summary>
public enum MonitoringSubjectType
{
    Producer,
    Product,
    Order,
    Payment,
    QrCode,
    Review,
    Village,
    District,
    Other,
}
