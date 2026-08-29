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

    public static DeliveryRouteDto ToDto(this DeliveryRoute r) => new()
    {
        Id = r.Id,
        RouteCode = r.RouteCode,
        LogisticsPartnerProfileId = r.LogisticsPartnerProfileId,
        LogisticsPartnerName = r.Profile?.CompanyName,
        CreatedByUserId = r.CreatedByUserId,
        CreatedByName = r.CreatedBy?.FullName,
        Name = r.Name,
        Status = r.Status.ToString(),
        ScheduledDate = r.ScheduledDate,
        PlannedStartAt = r.PlannedStartAt,
        PlannedEndAt = r.PlannedEndAt,
        ActualStartAt = r.ActualStartAt,
        ActualEndAt = r.ActualEndAt,
        StartLocationLabel = r.StartLocationLabel,
        StartLatitude = r.StartLatitude,
        StartLongitude = r.StartLongitude,
        EndLocationLabel = r.EndLocationLabel,
        EndLatitude = r.EndLatitude,
        EndLongitude = r.EndLongitude,
        OriginDistrictId = r.OriginDistrictId,
        OriginDistrictName = r.OriginDistrict?.Name,
        AssignedDriverName = r.AssignedDriverName,
        AssignedDriverPhone = r.AssignedDriverPhone,
        AssignedVehicleLabel = r.AssignedVehicleLabel,
        VehicleCapacityKg = r.VehicleCapacityKg,
        AssignedAt = r.AssignedAt,
        TotalStops = r.TotalStops,
        CompletedStops = r.CompletedStops,
        TotalLoadKg = r.TotalLoadKg,
        TotalDistanceKm = r.TotalDistanceKm,
        EstimatedDurationMinutes = r.EstimatedDurationMinutes,
        OptimizationStrategy = r.OptimizationStrategy,
        Notes = r.Notes,
        CancellationReason = r.CancellationReason,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Stops = r.Stops
            .OrderBy(s => s.Sequence)
            .Select(s => s.ToDto())
            .ToList(),
        Events = r.Events
            .OrderBy(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToList(),
    };

    public static DeliveryRouteListItemDto ToListItemDto(this DeliveryRoute r) => new()
    {
        Id = r.Id,
        RouteCode = r.RouteCode,
        Name = r.Name,
        Status = r.Status.ToString(),
        ScheduledDate = r.ScheduledDate,
        AssignedDriverName = r.AssignedDriverName,
        AssignedVehicleLabel = r.AssignedVehicleLabel,
        TotalStops = r.TotalStops,
        CompletedStops = r.CompletedStops,
        TotalLoadKg = r.TotalLoadKg,
        TotalDistanceKm = r.TotalDistanceKm,
        OptimizationStrategy = r.OptimizationStrategy,
        CreatedAt = r.CreatedAt,
    };

    public static RouteStopDto ToDto(this DeliveryRouteStop s) => new()
    {
        Id = s.Id,
        Sequence = s.Sequence,
        StopType = s.StopType.ToString(),
        Status = s.Status.ToString(),
        PickupRequestId = s.PickupRequestId,
        PickupReferenceCode = s.PickupRequest?.ReferenceCode,
        OrderId = s.OrderId,
        OrderNumber = s.Order?.OrderNumber,
        ContactName = s.ContactName,
        ContactPhone = s.ContactPhone,
        AddressLine = s.AddressLine,
        City = s.City,
        DistrictId = s.DistrictId,
        DistrictName = s.District?.Name,
        PostalCode = s.PostalCode,
        Latitude = s.Latitude,
        Longitude = s.Longitude,
        LoadKg = s.LoadKg,
        PackageCount = s.PackageCount,
        PlannedArrivalAt = s.PlannedArrivalAt,
        PlannedDepartureAt = s.PlannedDepartureAt,
        ActualArrivalAt = s.ActualArrivalAt,
        ActualDepartureAt = s.ActualDepartureAt,
        ServiceDurationMinutes = s.ServiceDurationMinutes,
        DistanceFromPreviousKm = s.DistanceFromPreviousKm,
        Instructions = s.Instructions,
        CompletionNote = s.CompletionNote,
        FailureReason = s.FailureReason,
    };

    public static RouteEventDto ToDto(this DeliveryRouteEvent e) => new()
    {
        Id = e.Id,
        Type = e.Type.ToString(),
        RouteStopId = e.RouteStopId,
        FromStatus = e.FromStatus?.ToString(),
        ToStatus = e.ToStatus?.ToString(),
        Note = e.Note,
        ActorUserId = e.ActorUserId,
        ActorName = e.Actor?.FullName,
        CreatedAt = e.CreatedAt,
    };

    public static ShipmentDto ToDto(this Shipment s) => new()
    {
        Id = s.Id,
        TrackingNumber = s.TrackingNumber,
        LogisticsPartnerProfileId = s.LogisticsPartnerProfileId,
        LogisticsPartnerName = s.Profile?.CompanyName,
        CreatedByUserId = s.CreatedByUserId,
        CreatedByName = s.CreatedBy?.FullName,
        Status = s.Status.ToString(),
        ServiceLevel = s.ServiceLevel.ToString(),
        OrderId = s.OrderId,
        OrderNumber = s.Order?.OrderNumber,
        PickupRequestId = s.PickupRequestId,
        PickupReferenceCode = s.PickupRequest?.ReferenceCode,
        DeliveryRouteId = s.DeliveryRouteId,
        DeliveryRouteCode = s.DeliveryRoute?.RouteCode,
        OriginContactName = s.OriginContactName,
        OriginPhone = s.OriginPhone,
        OriginAddressLine = s.OriginAddressLine,
        OriginCity = s.OriginCity,
        OriginDistrictId = s.OriginDistrictId,
        OriginDistrictName = s.OriginDistrict?.Name,
        OriginPostalCode = s.OriginPostalCode,
        RecipientName = s.RecipientName,
        RecipientPhone = s.RecipientPhone,
        DestinationAddressLine = s.DestinationAddressLine,
        DestinationCity = s.DestinationCity,
        DestinationDistrictId = s.DestinationDistrictId,
        DestinationDistrictName = s.DestinationDistrict?.Name,
        DestinationPostalCode = s.DestinationPostalCode,
        ParcelCount = s.ParcelCount,
        TotalWeightKg = s.TotalWeightKg,
        DimensionsNote = s.DimensionsNote,
        DeclaredValue = s.DeclaredValue,
        ShippingCost = s.ShippingCost,
        IsCashOnDelivery = s.IsCashOnDelivery,
        CodAmount = s.CodAmount,
        CodCollected = s.CodCollected,
        CodCollectedAt = s.CodCollectedAt,
        CurrentLocationLabel = s.CurrentLocationLabel,
        CurrentLatitude = s.CurrentLatitude,
        CurrentLongitude = s.CurrentLongitude,
        EstimatedDeliveryAt = s.EstimatedDeliveryAt,
        DispatchedAt = s.DispatchedAt,
        DeliveredAt = s.DeliveredAt,
        LastStatusAt = s.LastStatusAt,
        DeliveryAttemptCount = s.DeliveryAttemptCount,
        ReceivedByName = s.ReceivedByName,
        ProofOfDeliveryNote = s.ProofOfDeliveryNote,
        SignatureImageUrl = s.SignatureImageUrl,
        FailureReason = s.FailureReason,
        CancellationReason = s.CancellationReason,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        Events = s.Events
            .OrderBy(e => e.OccurredAt)
            .ThenBy(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToList(),
        Attempts = s.Attempts
            .OrderBy(a => a.AttemptNumber)
            .Select(a => a.ToDto())
            .ToList(),
    };

    public static ShipmentListItemDto ToListItemDto(this Shipment s) => new()
    {
        Id = s.Id,
        TrackingNumber = s.TrackingNumber,
        Status = s.Status.ToString(),
        ServiceLevel = s.ServiceLevel.ToString(),
        RecipientName = s.RecipientName,
        DestinationCity = s.DestinationCity,
        DestinationDistrictName = s.DestinationDistrict?.Name,
        ParcelCount = s.ParcelCount,
        IsCashOnDelivery = s.IsCashOnDelivery,
        EstimatedDeliveryAt = s.EstimatedDeliveryAt,
        LastStatusAt = s.LastStatusAt,
        OrderId = s.OrderId,
        OrderNumber = s.Order?.OrderNumber,
        CreatedAt = s.CreatedAt,
    };

    public static ShipmentTrackingEventDto ToDto(this ShipmentTrackingEvent e) => new()
    {
        Id = e.Id,
        EventType = e.EventType.ToString(),
        FromStatus = e.FromStatus?.ToString(),
        ToStatus = e.ToStatus?.ToString(),
        LocationLabel = e.LocationLabel,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        DistrictId = e.DistrictId,
        DistrictName = e.District?.Name,
        Description = e.Description,
        OccurredAt = e.OccurredAt,
        RecordedByUserId = e.RecordedByUserId,
        RecordedByName = e.RecordedBy?.FullName,
        CreatedAt = e.CreatedAt,
    };

    public static DeliveryAttemptDto ToDto(this DeliveryAttempt a) => new()
    {
        Id = a.Id,
        AttemptNumber = a.AttemptNumber,
        Outcome = a.Outcome.ToString(),
        AttemptedAt = a.AttemptedAt,
        Note = a.Note,
        NextAttemptAt = a.NextAttemptAt,
        RecordedByUserId = a.RecordedByUserId,
        RecordedByName = a.RecordedBy?.FullName,
        CreatedAt = a.CreatedAt,
    };

    public static ShipmentTrackingDto ToTrackingDto(this Shipment s) => new()
    {
        TrackingNumber = s.TrackingNumber,
        Status = s.Status.ToString(),
        ServiceLevel = s.ServiceLevel.ToString(),
        OriginCity = s.OriginCity,
        DestinationCity = s.DestinationCity,
        DestinationDistrictName = s.DestinationDistrict?.Name,
        ParcelCount = s.ParcelCount,
        EstimatedDeliveryAt = s.EstimatedDeliveryAt,
        DispatchedAt = s.DispatchedAt,
        DeliveredAt = s.DeliveredAt,
        LastStatusAt = s.LastStatusAt,
        CurrentLocationLabel = s.CurrentLocationLabel,
        Checkpoints = s.Events
            .OrderBy(e => e.OccurredAt)
            .ThenBy(e => e.CreatedAt)
            .Select(e => new ShipmentTrackingCheckpointDto
            {
                EventType = e.EventType.ToString(),
                Status = e.ToStatus?.ToString(),
                LocationLabel = e.LocationLabel,
                DistrictName = e.District?.Name,
                Description = e.Description,
                OccurredAt = e.OccurredAt,
            })
            .ToList(),
    };

    // ---- Warehouse management -------------------------------------

    public static WarehouseDto ToDto(this Warehouse w, int stockItemCount = 0) => new()
    {
        Id = w.Id,
        Code = w.Code,
        LogisticsPartnerProfileId = w.LogisticsPartnerProfileId,
        LogisticsPartnerName = w.Profile?.CompanyName,
        CreatedByUserId = w.CreatedByUserId,
        CreatedByName = w.CreatedBy?.FullName,
        Name = w.Name,
        Type = w.Type.ToString(),
        Status = w.Status.ToString(),
        AddressLine = w.AddressLine,
        City = w.City,
        DistrictId = w.DistrictId,
        DistrictName = w.District?.Name,
        PostalCode = w.PostalCode,
        Latitude = w.Latitude,
        Longitude = w.Longitude,
        ContactPersonName = w.ContactPersonName,
        ContactPhone = w.ContactPhone,
        TotalCapacityUnits = w.TotalCapacityUnits,
        UsedCapacityUnits = w.UsedCapacityUnits,
        HasColdChain = w.HasColdChain,
        HandlesHazardous = w.HandlesHazardous,
        HandlesReturns = w.HandlesReturns,
        Notes = w.Notes,
        ZoneCount = w.Zones.Count,
        BinCount = w.Bins.Count,
        StockItemCount = stockItemCount,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        Zones = w.Zones
            .OrderBy(z => z.Code)
            .Select(z => z.ToDto())
            .ToList(),
        Bins = w.Bins
            .OrderBy(b => b.Code)
            .Select(b => b.ToDto())
            .ToList(),
    };

    public static WarehouseListItemDto ToListItemDto(this Warehouse w, int zoneCount, int binCount) => new()
    {
        Id = w.Id,
        Code = w.Code,
        Name = w.Name,
        Type = w.Type.ToString(),
        Status = w.Status.ToString(),
        City = w.City,
        DistrictName = w.District?.Name,
        TotalCapacityUnits = w.TotalCapacityUnits,
        UsedCapacityUnits = w.UsedCapacityUnits,
        HasColdChain = w.HasColdChain,
        ZoneCount = zoneCount,
        BinCount = binCount,
        CreatedAt = w.CreatedAt,
    };

    public static WarehouseZoneDto ToDto(this WarehouseZone z) => new()
    {
        Id = z.Id,
        Code = z.Code,
        Name = z.Name,
        Type = z.Type.ToString(),
        IsColdChain = z.IsColdChain,
        CapacityUnits = z.CapacityUnits,
        IsActive = z.IsActive,
        Notes = z.Notes,
    };

    public static WarehouseBinDto ToDto(this WarehouseBin b) => new()
    {
        Id = b.Id,
        WarehouseZoneId = b.WarehouseZoneId,
        ZoneCode = b.Zone?.Code,
        Code = b.Code,
        Label = b.Label,
        Type = b.Type.ToString(),
        CapacityUnits = b.CapacityUnits,
        OccupiedUnits = b.OccupiedUnits,
        IsPickable = b.IsPickable,
        IsActive = b.IsActive,
    };

    public static WarehouseStockItemDto ToDto(this WarehouseStockItem i, IEnumerable<WarehouseStockMovement>? movements = null) => new()
    {
        Id = i.Id,
        WarehouseId = i.WarehouseId,
        WarehouseCode = i.Warehouse?.Code,
        WarehouseName = i.Warehouse?.Name,
        WarehouseBinId = i.WarehouseBinId,
        BinCode = i.Bin?.Code,
        ProductId = i.ProductId,
        ProductName = i.Product?.Name,
        OwnerUserId = i.OwnerUserId,
        OwnerName = i.Owner?.FullName,
        Sku = i.Sku,
        Description = i.Description,
        UnitOfMeasure = i.UnitOfMeasure,
        QuantityOnHand = i.QuantityOnHand,
        QuantityReserved = i.QuantityReserved,
        QuantityAvailable = i.QuantityAvailable,
        BatchNumber = i.BatchNumber,
        ExpiryDate = i.ExpiryDate,
        Status = i.Status.ToString(),
        UnitValue = i.UnitValue,
        ReceivedAt = i.ReceivedAt,
        LastMovementAt = i.LastMovementAt,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
        Movements = (movements ?? Enumerable.Empty<WarehouseStockMovement>())
            .OrderByDescending(m => m.OccurredAt)
            .Select(m => m.ToDto())
            .ToList(),
    };

    public static WarehouseStockItemListItemDto ToListItemDto(this WarehouseStockItem i) => new()
    {
        Id = i.Id,
        WarehouseId = i.WarehouseId,
        BinCode = i.Bin?.Code,
        Sku = i.Sku,
        Description = i.Description,
        UnitOfMeasure = i.UnitOfMeasure,
        QuantityOnHand = i.QuantityOnHand,
        QuantityReserved = i.QuantityReserved,
        QuantityAvailable = i.QuantityAvailable,
        BatchNumber = i.BatchNumber,
        ExpiryDate = i.ExpiryDate,
        Status = i.Status.ToString(),
        ProductName = i.Product?.Name,
        LastMovementAt = i.LastMovementAt,
    };

    public static WarehouseStockMovementDto ToDto(this WarehouseStockMovement m) => new()
    {
        Id = m.Id,
        WarehouseId = m.WarehouseId,
        WarehouseStockItemId = m.WarehouseStockItemId,
        Type = m.Type.ToString(),
        Quantity = m.Quantity,
        QuantityOnHandAfter = m.QuantityOnHandAfter,
        FromBinId = m.FromBinId,
        ToBinId = m.ToBinId,
        Sku = m.Sku,
        ReferenceType = m.ReferenceType,
        ReferenceId = m.ReferenceId,
        Reason = m.Reason,
        Note = m.Note,
        PerformedByUserId = m.PerformedByUserId,
        PerformedByName = m.PerformedBy?.FullName,
        OccurredAt = m.OccurredAt,
        CreatedAt = m.CreatedAt,
    };
}
