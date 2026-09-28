namespace ShilpoHubBD.Domain.Entities.ProducerPartnership;

public enum ProducerPartnershipSettlementFrequency
{
    Monthly,
    Quarterly,
    Biannual,

    /// <summary>Admin-defined period length — see <see cref="ProducerPartnershipAgreement.CustomSettlementPeriodDays"/>.</summary>
    Custom,
}
