using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// A district a <see cref="LogisticsPartnerProfile"/> operates in, with the service terms that apply
/// there. Denormalised district name / division so the coverage list survives a district rename or
/// deactivation.
/// </summary>
public class LogisticsServiceArea
{
    public Guid Id { get; set; }

    public Guid LogisticsPartnerProfileId { get; set; }
    public LogisticsPartnerProfile Profile { get; set; } = null!;

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;

    /// <summary>Typical door-to-door transit time within / to this district, in days.</summary>
    public int StandardDeliveryDays { get; set; } = 3;

    public bool SupportsSameDay { get; set; }

    /// <summary>Optional flat surcharge applied to shipments touching this area.</summary>
    public decimal? SurchargeAmount { get; set; }

    public bool IsActive { get; set; } = true;
}
