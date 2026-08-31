using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>
/// Operational profile for a Logistics Partner user — the company that runs pickups, routes,
/// deliveries, warehouses and returns on the platform. One profile per <see cref="User"/> in the
/// LogisticsPartner role. Service areas, fleet capacity and operating hours here feed the pickup
/// scheduler and (later parts) route optimisation and warehouse allocation.
/// </summary>
public class LogisticsPartnerProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // ---- Company -----------------------------------------------------------
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }

    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    // ---- Operations base ------------------------------------------------
    public string BaseAddressLine { get; set; } = string.Empty;
    public string BaseCity { get; set; } = string.Empty;
    public Guid? BaseDistrictId { get; set; }
    public District? BaseDistrict { get; set; }
    public string? BasePostalCode { get; set; }
    public string Country { get; set; } = "Bangladesh";

    // ---- Capacity ------------------------------------------------------
    public int FleetSize { get; set; }
    public int MaxDailyPickups { get; set; }
    public decimal? MaxVehicleCapacityKg { get; set; }

    /// <summary>Operating window start hour (0-24, local time). Null = unspecified.</summary>
    public int? OperatingDayStartHour { get; set; }

    /// <summary>Operating window end hour (0-24, local time). Null = unspecified.</summary>
    public int? OperatingDayEndHour { get; set; }

    // ---- Capabilities ------------------------------------------------
    public bool OffersCashOnDelivery { get; set; }
    public bool OffersColdChain { get; set; }
    public bool OffersFragileHandling { get; set; }

    // ---- Status ----------------------------------------------------
    public bool IsAcceptingRequests { get; set; } = true;

    public LogisticsPartnerVerificationStatus VerificationStatus { get; set; }
        = LogisticsPartnerVerificationStatus.Pending;

    public Guid? VerifiedByUserId { get; set; }
    public User? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LogisticsServiceArea> ServiceAreas { get; set; } = new List<LogisticsServiceArea>();
}
