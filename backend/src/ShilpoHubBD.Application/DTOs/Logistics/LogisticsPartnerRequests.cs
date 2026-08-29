namespace ShilpoHubBD.Application.DTOs.Logistics;

public class UpsertLogisticsPartnerProfileRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }

    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public string BaseAddressLine { get; set; } = string.Empty;
    public string BaseCity { get; set; } = string.Empty;
    public Guid? BaseDistrictId { get; set; }
    public string? BasePostalCode { get; set; }
    public string? Country { get; set; }

    public int FleetSize { get; set; }
    public int MaxDailyPickups { get; set; }
    public decimal? MaxVehicleCapacityKg { get; set; }
    public int? OperatingDayStartHour { get; set; }
    public int? OperatingDayEndHour { get; set; }

    public bool OffersCashOnDelivery { get; set; }
    public bool OffersColdChain { get; set; }
    public bool OffersFragileHandling { get; set; }

    public bool IsAcceptingRequests { get; set; } = true;
    public string? Notes { get; set; }
}

public class VerifyLogisticsPartnerRequest
{
    /// <summary>Pending, Verified, Rejected or Suspended.</summary>
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpsertLogisticsServiceAreaRequest
{
    public Guid DistrictId { get; set; }
    public int StandardDeliveryDays { get; set; } = 3;
    public bool SupportsSameDay { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LogisticsPartnerQueryParameters
{
    /// <summary>Pending, Verified, Rejected or Suspended.</summary>
    public string? VerificationStatus { get; set; }
    public bool? IsAcceptingRequests { get; set; }
    public Guid? ServiceDistrictId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
