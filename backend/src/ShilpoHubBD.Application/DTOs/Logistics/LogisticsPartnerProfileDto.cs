namespace ShilpoHubBD.Application.DTOs.Logistics;

public class LogisticsPartnerProfileDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }

    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public string BaseAddressLine { get; set; } = string.Empty;
    public string BaseCity { get; set; } = string.Empty;
    public Guid? BaseDistrictId { get; set; }
    public string? BaseDistrictName { get; set; }
    public string? BasePostalCode { get; set; }
    public string Country { get; set; } = "Bangladesh";

    public int FleetSize { get; set; }
    public int MaxDailyPickups { get; set; }
    public decimal? MaxVehicleCapacityKg { get; set; }
    public int? OperatingDayStartHour { get; set; }
    public int? OperatingDayEndHour { get; set; }

    public bool OffersCashOnDelivery { get; set; }
    public bool OffersColdChain { get; set; }
    public bool OffersFragileHandling { get; set; }
    public bool OffersPickup { get; set; }
    public bool SupportsReturns { get; set; }

    public bool IsAcceptingRequests { get; set; }
    public bool IsActive { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public Guid? VerifiedByUserId { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<LogisticsServiceAreaDto> ServiceAreas { get; set; } = new();
}

public class LogisticsPartnerProfileListItemDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string BaseCity { get; set; } = string.Empty;
    public int FleetSize { get; set; }
    public bool IsAcceptingRequests { get; set; }
    public bool IsActive { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public int ServiceAreaCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LogisticsServiceAreaDto
{
    public Guid Id { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string? AreaName { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public int StandardDeliveryDays { get; set; }
    public bool SupportsSameDay { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public bool PickupAvailable { get; set; }
    public bool ReturnSupported { get; set; }
    public bool IsActive { get; set; }
}

public class LogisticsPartnerPerformanceDto
{
    public Guid PartnerId { get; set; }
    public int TotalDeliveries { get; set; }
    public int Delivered { get; set; }
    public int InTransit { get; set; }
    public int Failed { get; set; }
    public int Cancelled { get; set; }
    public int Returned { get; set; }
    public decimal SuccessRate { get; set; }
    public double? AverageDeliveryHours { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ShilpoHubRevenue { get; set; }
    public decimal PartnerRevenue { get; set; }
    public List<LogisticsAreaPerformanceDto> Areas { get; set; } = new();
}

public class LogisticsAreaPerformanceDto
{
    public string Area { get; set; } = string.Empty;
    public int Deliveries { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
}
