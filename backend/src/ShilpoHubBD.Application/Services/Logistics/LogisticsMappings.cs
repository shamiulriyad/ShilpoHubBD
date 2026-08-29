using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

internal static class LogisticsMappings
{
    public static LogisticsPartnerProfileDto ToDto(this LogisticsPartnerProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        UserName = p.User?.FullName,
        CompanyName = p.CompanyName,
        LegalName = p.LegalName,
        RegistrationNumber = p.RegistrationNumber,
        ContactPersonName = p.ContactPersonName,
        ContactPhone = p.ContactPhone,
        ContactEmail = p.ContactEmail,
        BaseAddressLine = p.BaseAddressLine,
        BaseCity = p.BaseCity,
        BaseDistrictId = p.BaseDistrictId,
        BaseDistrictName = p.BaseDistrict?.Name,
        BasePostalCode = p.BasePostalCode,
        Country = p.Country,
        FleetSize = p.FleetSize,
        MaxDailyPickups = p.MaxDailyPickups,
        MaxVehicleCapacityKg = p.MaxVehicleCapacityKg,
        OperatingDayStartHour = p.OperatingDayStartHour,
        OperatingDayEndHour = p.OperatingDayEndHour,
        OffersCashOnDelivery = p.OffersCashOnDelivery,
        OffersColdChain = p.OffersColdChain,
        OffersFragileHandling = p.OffersFragileHandling,
        IsAcceptingRequests = p.IsAcceptingRequests,
        VerificationStatus = p.VerificationStatus.ToString(),
        VerifiedByUserId = p.VerifiedByUserId,
        VerifiedByName = p.VerifiedBy?.FullName,
        VerifiedAt = p.VerifiedAt,
        VerificationNotes = p.VerificationNotes,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        ServiceAreas = p.ServiceAreas
            .OrderBy(a => a.Division)
            .ThenBy(a => a.DistrictName)
            .Select(a => a.ToDto())
            .ToList(),
    };

    public static LogisticsPartnerProfileListItemDto ToListItemDto(this LogisticsPartnerProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        CompanyName = p.CompanyName,
        ContactPhone = p.ContactPhone,
        BaseCity = p.BaseCity,
        FleetSize = p.FleetSize,
        IsAcceptingRequests = p.IsAcceptingRequests,
        VerificationStatus = p.VerificationStatus.ToString(),
        ServiceAreaCount = p.ServiceAreas.Count,
        CreatedAt = p.CreatedAt,
    };

    public static LogisticsServiceAreaDto ToDto(this LogisticsServiceArea a) => new()
    {
        Id = a.Id,
        DistrictId = a.DistrictId,
        DistrictName = a.DistrictName,
        Division = a.Division,
        StandardDeliveryDays = a.StandardDeliveryDays,
        SupportsSameDay = a.SupportsSameDay,
        SurchargeAmount = a.SurchargeAmount,
        IsActive = a.IsActive,
    };

    public static PickupRequestDto ToDto(this PickupRequest r) => new()
    {
        Id = r.Id,
        ReferenceCode = r.ReferenceCode,
        LogisticsPartnerProfileId = r.LogisticsPartnerProfileId,
        LogisticsPartnerName = r.Profile?.CompanyName,
        RequestedByUserId = r.RequestedByUserId,
        RequestedByName = r.RequestedBy?.FullName,
        Status = r.Status.ToString(),
        Priority = r.Priority.ToString(),
        OrderId = r.OrderId,
        OrderNumber = r.Order?.OrderNumber,
        OriginContactName = r.OriginContactName,
        OriginPhone = r.OriginPhone,
        OriginAddressLine = r.OriginAddressLine,
        OriginCity = r.OriginCity,
        OriginDistrictId = r.OriginDistrictId,
        OriginDistrictName = r.OriginDistrict?.Name,
        OriginPostalCode = r.OriginPostalCode,
        OriginProducerUserId = r.OriginProducerUserId,
        OriginProducerName = r.OriginProducer?.FullName,
        DestinationContactName = r.DestinationContactName,
        DestinationPhone = r.DestinationPhone,
        DestinationAddressLine = r.DestinationAddressLine,
        DestinationCity = r.DestinationCity,
        DestinationDistrictId = r.DestinationDistrictId,
        DestinationDistrictName = r.DestinationDistrict?.Name,
        ScheduledPickupAt = r.ScheduledPickupAt,
        PickupWindowEnd = r.PickupWindowEnd,
        ActualPickupAt = r.ActualPickupAt,
        PackageCount = r.PackageCount,
        TotalWeightKg = r.TotalWeightKg,
        DeclaredValue = r.DeclaredValue,
        RequiresColdChain = r.RequiresColdChain,
        IsFragile = r.IsFragile,
        IsCashOnDelivery = r.IsCashOnDelivery,
        CodAmount = r.CodAmount,
        AssignedDriverName = r.AssignedDriverName,
        AssignedDriverPhone = r.AssignedDriverPhone,
        AssignedVehicleLabel = r.AssignedVehicleLabel,
        AssignedAt = r.AssignedAt,
        SpecialInstructions = r.SpecialInstructions,
        CancellationReason = r.CancellationReason,
        FailureReason = r.FailureReason,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Items = r.Items
            .OrderBy(i => i.Description)
            .Select(i => i.ToDto())
            .ToList(),
        Events = r.Events
            .OrderBy(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToList(),
    };

    public static PickupRequestListItemDto ToListItemDto(this PickupRequest r) => new()
    {
        Id = r.Id,
        ReferenceCode = r.ReferenceCode,
        Status = r.Status.ToString(),
        Priority = r.Priority.ToString(),
        OriginCity = r.OriginCity,
        OriginDistrictName = r.OriginDistrict?.Name,
        DestinationCity = r.DestinationCity,
        ScheduledPickupAt = r.ScheduledPickupAt,
        PackageCount = r.PackageCount,
        TotalWeightKg = r.TotalWeightKg,
        AssignedDriverName = r.AssignedDriverName,
        OrderId = r.OrderId,
        OrderNumber = r.Order?.OrderNumber,
        CreatedAt = r.CreatedAt,
    };

    public static PickupItemDto ToDto(this PickupItem i) => new()
    {
        Id = i.Id,
        Description = i.Description,
        Quantity = i.Quantity,
        WeightKg = i.WeightKg,
        LengthCm = i.LengthCm,
        WidthCm = i.WidthCm,
        HeightCm = i.HeightCm,
        Reference = i.Reference,
        IsFragile = i.IsFragile,
    };

    public static PickupEventDto ToDto(this PickupEvent e) => new()
    {
        Id = e.Id,
        Type = e.Type.ToString(),
        FromStatus = e.FromStatus?.ToString(),
        ToStatus = e.ToStatus?.ToString(),
        Note = e.Note,
        ActorUserId = e.ActorUserId,
        ActorName = e.Actor?.FullName,
        CreatedAt = e.CreatedAt,
    };
}
