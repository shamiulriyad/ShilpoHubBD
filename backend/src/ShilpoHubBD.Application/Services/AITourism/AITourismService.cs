using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AITourism;

public class AITourismService : IAITourismService
{
    private const decimal DefaultDailyFoodBudgetPerPerson = 600m;
    private const decimal DefaultDailyMiscBudgetPerPerson = 300m;

    private readonly IAITourismProvider _aiTourismProvider;
    private readonly IGeocodingProvider _geocodingProvider;
    private readonly IRoutingProvider _routingProvider;
    private readonly ITravelPlannerRagProvider _travelPlannerRagProvider;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;
    private readonly IHeritageFestivalRepository _heritageFestivalRepository;
    private readonly ICulturalEventRepository _culturalEventRepository;
    private readonly ILocalCuisineRepository _localCuisineRepository;
    private readonly ITouristServiceRepository _touristServiceRepository;
    private readonly ITourismLocationRepository _tourismLocationRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly ITransportOptionRepository _transportOptionRepository;

    public AITourismService(
        IAITourismProvider aiTourismProvider,
        IGeocodingProvider geocodingProvider,
        IRoutingProvider routingProvider,
        ITravelPlannerRagProvider travelPlannerRagProvider,
        IHeritagePlaceRepository heritagePlaceRepository,
        IHeritageFestivalRepository heritageFestivalRepository,
        ICulturalEventRepository culturalEventRepository,
        ILocalCuisineRepository localCuisineRepository,
        ITouristServiceRepository touristServiceRepository,
        ITourismLocationRepository tourismLocationRepository,
        IDistrictRepository districtRepository,
        ITransportOptionRepository transportOptionRepository)
    {
        _transportOptionRepository = transportOptionRepository;
        _aiTourismProvider = aiTourismProvider;
        _geocodingProvider = geocodingProvider;
        _routingProvider = routingProvider;
        _travelPlannerRagProvider = travelPlannerRagProvider;
        _heritagePlaceRepository = heritagePlaceRepository;
        _heritageFestivalRepository = heritageFestivalRepository;
        _culturalEventRepository = culturalEventRepository;
        _localCuisineRepository = localCuisineRepository;
        _touristServiceRepository = touristServiceRepository;
        _tourismLocationRepository = tourismLocationRepository;
        _districtRepository = districtRepository;
    }

    public async Task<TourPlanResult> PlanTourAsync(TourPlanRequest request, CancellationToken cancellationToken)
    {
        var districtName = await ResolveDistrictNameAsync(request.DistrictId, cancellationToken);

        var (places, _) = await _heritagePlaceRepository.GetPagedAsync(
            new HeritagePlaceQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (festivals, _) = await _heritageFestivalRepository.GetPagedAsync(
            new HeritageFestivalQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (events, _) = await _culturalEventRepository.GetPagedAsync(
            new CulturalEventQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (services, _) = await _touristServiceRepository.GetPagedAsync(
            new TouristServiceQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (tourismLocations, _) = await _tourismLocationRepository.GetPagedAsync(
            new TourismLocationQueryParameters { DistrictId = request.DistrictId, IsActive = true, PageSize = 50 }, cancellationToken);

        var transportEstimate = await BuildTransportInfoAsync(request, districtName, cancellationToken);
        await AttachTransportOptionsAsync(transportEstimate, request, districtName, cancellationToken);

        var ragNotes = await _travelPlannerRagProvider.RetrieveAsync(
            $"Tourist places and activities in {districtName}", request.DistrictId.HasValue ? districtName : null,
            request.Preferences, cancellationToken);

        var context = new TourPlanContext
        {
            DistrictName = districtName,
            DurationDays = Math.Clamp(request.DurationDays, 1, 30),
            PartySize = Math.Max(1, request.PartySize),
            StartDate = request.StartDate,
            OriginText = request.OriginText ?? string.Empty,
            TransportMode = string.IsNullOrWhiteSpace(request.TransportMode) ? "Bus" : request.TransportMode,
            Budget = request.Budget,
            Preferences = request.Preferences ?? new(),
            Places = places.Select(ToPlaceSummary).ToList(),
            Festivals = festivals.Select(ToFestivalSummary).ToList(),
            Events = events.Select(ToEventSummary).ToList(),
            Services = services.Select(ToServiceSummary).ToList(),
            TourismLocations = tourismLocations.Select(ToTourismLocationSummary).ToList(),
            RagNotes = ragNotes,
            TransportEstimate = transportEstimate,
        };

        var result = await _aiTourismProvider.PlanTourAsync(context, cancellationToken);
        result.TransportEstimate = transportEstimate;
        await EnrichStopCoordinatesAsync(result, context, cancellationToken);
        result.EstimatedBudget = await BuildEstimatedBudgetAsync(result, services, context, cancellationToken);
        return result;
    }

    // Real geocoding (Nominatim) for both ends, and real road routing (OSRM) for Bus only -- OSRM
    // only has road profiles, so drawing a "driving" line for a Train/Plane trip would itself be
    // the kind of fake route this must not produce. Never falls back to a guessed straight line.
    private async Task<TransportEstimateDto> BuildTransportInfoAsync(
        TourPlanRequest request, string destinationName, CancellationToken cancellationToken)
    {
        var mode = string.IsNullOrWhiteSpace(request.TransportMode) ? "Bus" : request.TransportMode;

        GeoPointDto? origin = null;
        if (!string.IsNullOrWhiteSpace(request.OriginText))
        {
            origin = await _geocodingProvider.GeocodeAsync(request.OriginText, cancellationToken);
        }

        var destination = await _geocodingProvider.GeocodeAsync($"{destinationName}, Bangladesh", cancellationToken);

        var estimate = new TransportEstimateDto { Mode = mode, OriginPoint = origin, DestinationPoint = destination };

        if (!string.IsNullOrWhiteSpace(request.OriginText) && origin is null)
        {
            estimate.Notes = $"Could not find \"{request.OriginText}\" -- try a different spelling or a nearby well-known place.";
            return estimate;
        }

        if (destination is null)
        {
            estimate.Notes = $"Could not find \"{destinationName}\" -- try another district.";
            return estimate;
        }

        if (!string.Equals(mode, "Bus", StringComparison.OrdinalIgnoreCase))
        {
            estimate.Notes = $"No verified {mode.ToLowerInvariant()} schedule source is configured yet. " +
                "This mode is available to plan around, but timing and fares must be checked with the operator.";
            return estimate;
        }

        if (origin is null)
        {
            estimate.Notes = "Add a starting point to get a road distance and travel time estimate.";
            return estimate;
        }

        var route = await _routingProvider.GetDrivingRouteAsync(origin, destination, cancellationToken);
        if (route is null)
        {
            estimate.Notes = "The routing service could not calculate a road route right now -- try again shortly.";
            return estimate;
        }

        estimate.EstimatedDistanceKm = route.DistanceKm;
        estimate.EstimatedDurationMinutes = route.DurationMinutes;
        estimate.RouteGeometry = route.Geometry;
        estimate.Notes = "Estimated using real road-network routing (OSRM) -- not a live bus schedule; confirm timing and fares with the operator.";
        return estimate;
    }

    // Attaches real coordinates to each generated stop so the map can plot it: curated places and
    // services already carry their own DB coordinates; anything else (an AI-suggested "Attraction"
    // outside the curated tables) is geocoded by name and cached, never fabricated.
    private async Task EnrichStopCoordinatesAsync(TourPlanResult result, TourPlanContext context, CancellationToken cancellationToken)
    {
        var placesById = context.Places.ToDictionary(p => p.Id);
        var servicesById = context.Services.ToDictionary(s => s.Id);
        var locationsById = context.TourismLocations.ToDictionary(l => l.Id);
        var geocodedByName = new Dictionary<string, GeoPointDto?>(StringComparer.OrdinalIgnoreCase);

        foreach (var stop in result.Days.SelectMany(d => d.Stops))
        {
            // The id is what was grounded against the database; the model's label for it can be wrong
            // (e.g. a curated tourism location typed "HeritagePlace"), which would drop its coordinates.
            if (stop.ReferenceId is { } refId)
            {
                if (locationsById.ContainsKey(refId)) stop.Type = "TourismLocation";
                else if (placesById.ContainsKey(refId)) stop.Type = "HeritagePlace";
                else if (servicesById.ContainsKey(refId)) stop.Type = "TouristService";
            }

            if (stop.ReferenceId.HasValue && stop.Type == "HeritagePlace" && placesById.TryGetValue(stop.ReferenceId.Value, out var place))
            {
                stop.Latitude = place.Latitude;
                stop.Longitude = place.Longitude;
                continue;
            }

            if (stop.ReferenceId.HasValue && stop.Type == "TouristService" && servicesById.TryGetValue(stop.ReferenceId.Value, out var service)
                && service.Latitude.HasValue && service.Longitude.HasValue)
            {
                stop.Latitude = service.Latitude;
                stop.Longitude = service.Longitude;
                continue;
            }

            if (stop.ReferenceId.HasValue && stop.Type == "TourismLocation" && locationsById.TryGetValue(stop.ReferenceId.Value, out var location))
            {
                stop.Latitude = location.Latitude;
                stop.Longitude = location.Longitude;
                continue;
            }

            if (stop.Type == "FreeTime" || string.IsNullOrWhiteSpace(stop.Name))
            {
                continue;
            }

            if (!geocodedByName.TryGetValue(stop.Name, out var geocoded))
            {
                geocoded = await _geocodingProvider.GeocodeAsync($"{stop.Name}, {context.DistrictName}, Bangladesh", cancellationToken);
                geocodedByName[stop.Name] = geocoded;
            }

            if (geocoded is not null)
            {
                stop.Latitude = geocoded.Latitude;
                stop.Longitude = geocoded.Longitude;
            }
        }
    }

    private async Task<BudgetPlanResult> BuildEstimatedBudgetAsync(
        TourPlanResult tourPlan,
        List<Domain.Entities.TouristBooking.TouristService> availableServices,
        TourPlanContext context,
        CancellationToken cancellationToken)
    {
        var referencedServiceIds = tourPlan.Days
            .SelectMany(d => d.Stops)
            .Where(s => s.Type == "TouristService" && s.ReferenceId.HasValue)
            .Select(s => s.ReferenceId!.Value)
            .Distinct()
            .ToList();

        var serviceLines = availableServices
            .Where(s => referencedServiceIds.Contains(s.Id))
            .Select(s => new BudgetServiceLineDto
            {
                Title = s.Title,
                Type = s.Type.ToString(),
                UnitPrice = s.Price,
                PartySize = context.PartySize,
            })
            .ToList();

        var budgetContext = new BudgetPlanContext
        {
            ServiceLines = serviceLines,
            DurationDays = context.DurationDays,
            PartySize = context.PartySize,
            DailyFoodBudgetPerPerson = DefaultDailyFoodBudgetPerPerson,
            DailyMiscBudgetPerPerson = DefaultDailyMiscBudgetPerPerson,
        };

        var budget = await _aiTourismProvider.PlanBudgetAsync(budgetContext, cancellationToken);
        AddTransportCost(budget, tourPlan.TransportEstimate, context);
        AddTourismLocationCosts(budget, tourPlan, context);
        return budget;
    }

    // The traveller's starting point is free text ("Dhaka", "Dhaka, Bangladesh"), so a route matches
    // when it contains the option's origin name. Only sourced options are listed; a mode or route with
    // no data simply has none, and the generic "no verified schedule" note stays.
    private async Task AttachTransportOptionsAsync(
        TransportEstimateDto estimate, TourPlanRequest request, string districtName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OriginText) || !request.DistrictId.HasValue)
        {
            return;
        }

        var rows = await _transportOptionRepository.GetForDestinationAsync(districtName, estimate.Mode, cancellationToken);
        var matched = rows
            .Where(r => request.OriginText.Contains(r.OriginName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (matched.Count == 0)
        {
            return;
        }

        estimate.Options = matched.Select(o => new TransportOptionDto
        {
            Mode = o.Mode,
            Operator = o.Operator,
            ServiceName = o.ServiceName,
            ServiceClasses = o.ServiceClasses,
            Schedule = o.Schedule,
            DurationText = o.DurationText,
            FareBdt = o.FareBdt,
            FareNote = o.FareNote,
            BookingUrl = o.BookingUrl,
            Notes = o.Notes,
            SourceUrl = o.SourceUrl,
            VerificationStatus = o.VerificationStatus,
            UnverifiedFields = o.UnverifiedFields,
            DataRetrievedOn = o.DataRetrievedOn,
        }).ToList();

        var duration = matched.Select(o => o.DurationText).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d));
        if (string.Equals(estimate.Mode, "Bus", StringComparison.OrdinalIgnoreCase))
        {
            // The OSRM figure is car speed; real buses are slower, and the sources say by how much.
            if (duration is not null)
            {
                estimate.Notes = $"{estimate.Notes} Buses take longer than a car: {duration}.".Trim();
            }
        }
        else
        {
            estimate.Notes = $"Services on this route are listed below. Timings and fares are not live -- confirm with the operator or the official booking site.";
        }
    }

    // Adds a round-trip transport line from the lowest reported fare. Nothing is added when no fare
    // is reported (a note says so instead), and a fare that is not from a verified source is labelled.
    private static void AddTransportCost(BudgetPlanResult budget, TransportEstimateDto? transport, TourPlanContext context)
    {
        if (transport is null || transport.Options.Count == 0)
        {
            return;
        }

        var cheapest = transport.Options.Where(o => o.FareBdt.HasValue).OrderBy(o => o.FareBdt).FirstOrDefault();
        if (cheapest is null)
        {
            budget.Notes = $"{budget.Notes} {transport.Mode} fares are not included: none is reported by the sources -- confirm with the operator.".Trim();
            return;
        }

        var verified = cheapest.VerificationStatus == "Verified";
        budget.LineItems.Add(new BudgetLineItemDto
        {
            Label = $"{transport.Mode} tickets ({cheapest.Operator}, lowest reported fare, {context.PartySize} traveller(s), round trip"
                + (verified ? ")" : "; fare unverified)"),
            Category = "Transport",
            Amount = cheapest.FareBdt!.Value * context.PartySize * 2,
        });
        if (!verified)
        {
            budget.Notes = $"{budget.Notes} The transport fare comes from a single unverified source and assumes the same fare for the return -- confirm before booking.".Trim();
        }
    }

    // Adds the sourced (TourismLocation) accommodation and entry-fee costs to the budget. Only prices
    // that exist in the data are used -- an item with no verified price is left out of the total and
    // named in the notes instead of being guessed.
    private static void AddTourismLocationCosts(BudgetPlanResult budget, TourPlanResult tourPlan, TourPlanContext context)
    {
        var locationsById = context.TourismLocations.ToDictionary(l => l.Id);
        var stops = tourPlan.Days.SelectMany(d => d.Stops).ToList();
        var referenced = stops
            .Where(s => s.ReferenceId.HasValue && locationsById.ContainsKey(s.ReferenceId.Value))
            .Select(s => locationsById[s.ReferenceId!.Value])
            .DistinctBy(l => l.Id)
            .ToList();
        var notes = new List<string>();

        static bool IsLodging(string type) => type is "Hotel" or "Resort" or "Hostel";

        var lodging = referenced.FirstOrDefault(l => IsLodging(l.Type))
            ?? (string.IsNullOrWhiteSpace(tourPlan.AccommodationRecommendation) ? null
                : context.TourismLocations.FirstOrDefault(l => IsLodging(l.Type)
                    && tourPlan.AccommodationRecommendation.Contains(l.Name, StringComparison.OrdinalIgnoreCase)));
        var nights = Math.Max(0, context.DurationDays - 1);
        if (lodging is not null && nights > 0)
        {
            if (lodging.Price.HasValue)
            {
                var rooms = (int)Math.Ceiling(context.PartySize / 2.0);
                budget.LineItems.Add(new BudgetLineItemDto
                {
                    Label = $"Accommodation: {lodging.Name} ({rooms} room(s) x {nights} night(s), lowest listed rate)",
                    Category = "Accommodation",
                    Amount = lodging.Price.Value * rooms * nights,
                });
            }
            else
            {
                notes.Add($"Accommodation ({lodging.Name}) is not in the total: no verified room rate is available.");
            }
        }

        var unpriced = new List<string>();
        foreach (var place in referenced.Where(l => !IsLodging(l.Type) && l.Type != "Restaurant"))
        {
            if (place.EntryFee.HasValue)
            {
                if (place.EntryFee.Value > 0)
                {
                    budget.LineItems.Add(new BudgetLineItemDto
                    {
                        Label = $"Entry fee: {place.Name} x {context.PartySize}",
                        Category = "Entry fees",
                        Amount = place.EntryFee.Value * context.PartySize,
                    });
                }
            }
            else
            {
                unpriced.Add(place.Name);
            }
        }

        if (unpriced.Count > 0)
        {
            notes.Add($"Entry fees not verified and not included: {string.Join(", ", unpriced)}.");
        }

        budget.TotalEstimatedCost = budget.LineItems.Sum(i => i.Amount);
        budget.PerPersonCost = context.PartySize > 0 ? Math.Round(budget.TotalEstimatedCost / context.PartySize, 2) : budget.TotalEstimatedCost;
        if (notes.Count > 0)
        {
            budget.Notes = string.Join(" ", new[] { budget.Notes }.Concat(notes).Where(n => !string.IsNullOrWhiteSpace(n)));
        }
    }

    public async Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanRequest request, CancellationToken cancellationToken)
    {
        var serviceIds = request.Selections.Select(s => s.ServiceId).Distinct().ToList();
        var services = serviceIds.Count == 0
            ? new List<Domain.Entities.TouristBooking.TouristService>()
            : await _touristServiceRepository.GetByIdsAsync(serviceIds, cancellationToken);

        var serviceLines = new List<BudgetServiceLineDto>();
        foreach (var selection in request.Selections)
        {
            var service = services.FirstOrDefault(s => s.Id == selection.ServiceId)
                ?? throw new NotFoundException($"Tourist service '{selection.ServiceId}' not found.");

            serviceLines.Add(new BudgetServiceLineDto
            {
                Title = service.Title,
                Type = service.Type.ToString(),
                UnitPrice = service.Price,
                PartySize = Math.Max(1, selection.PartySize),
            });
        }

        var context = new BudgetPlanContext
        {
            ServiceLines = serviceLines,
            DurationDays = Math.Clamp(request.DurationDays, 1, 90),
            PartySize = Math.Max(1, request.PartySize),
            DailyFoodBudgetPerPerson = request.DailyFoodBudgetPerPerson ?? DefaultDailyFoodBudgetPerPerson,
            DailyMiscBudgetPerPerson = request.DailyMiscBudgetPerPerson ?? DefaultDailyMiscBudgetPerPerson,
        };

        return await _aiTourismProvider.PlanBudgetAsync(context, cancellationToken);
    }

    public async Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationRequest request, CancellationToken cancellationToken)
    {
        var placeIds = request.PlaceIds.Distinct().ToList();
        var places = await _heritagePlaceRepository.GetByIdsAsync(placeIds, cancellationToken);

        var missing = placeIds.Except(places.Select(p => p.Id)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException($"Heritage place(s) not found: {string.Join(", ", missing)}.");
        }

        var context = new RouteOptimizationContext
        {
            Places = places.Select(p => new RoutePlaceDto { Id = p.Id, Name = p.Name, Latitude = p.Latitude, Longitude = p.Longitude }).ToList(),
            StartLatitude = request.StartLatitude,
            StartLongitude = request.StartLongitude,
        };

        return await _aiTourismProvider.OptimizeRouteAsync(context, cancellationToken);
    }

    public Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken)
        => _aiTourismProvider.TranslateAsync(request, cancellationToken);

    public async Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationRequest request, CancellationToken cancellationToken)
    {
        if (request.DistrictId.HasValue)
        {
            await ResolveDistrictNameAsync(request.DistrictId, cancellationToken);
        }

        var (places, _) = await _heritagePlaceRepository.GetPagedAsync(
            new HeritagePlaceQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (festivals, _) = await _heritageFestivalRepository.GetPagedAsync(
            new HeritageFestivalQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (events, _) = await _culturalEventRepository.GetPagedAsync(
            new CulturalEventQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var (cuisines, _) = await _localCuisineRepository.GetPagedAsync(
            new LocalCuisineQueryParameters { DistrictId = request.DistrictId, PageSize = 50 }, cancellationToken);

        var context = new CulturalRecommendationContext
        {
            Places = places.Select(ToPlaceSummary).ToList(),
            Festivals = festivals.Select(ToFestivalSummary).ToList(),
            Events = events.Select(ToEventSummary).ToList(),
            Cuisines = cuisines.Select(ToCuisineSummary).ToList(),
            Interests = request.Interests,
            MaxResults = Math.Clamp(request.MaxResults, 1, 50),
        };

        return await _aiTourismProvider.RecommendAsync(context, cancellationToken);
    }

    private async Task<string> ResolveDistrictNameAsync(Guid? districtId, CancellationToken cancellationToken)
    {
        if (!districtId.HasValue)
        {
            return "Bangladesh";
        }

        var district = await _districtRepository.GetByIdAsync(districtId.Value, cancellationToken)
            ?? throw new NotFoundException("District not found.");

        return district.Name;
    }

    private static HeritagePlaceSummaryDto ToPlaceSummary(Domain.Entities.HeritageDiscovery.HeritagePlace place) => new()
    {
        Id = place.Id,
        Name = place.Name,
        Description = place.Description,
        PlaceType = place.PlaceType.ToString(),
        Latitude = place.Latitude,
        Longitude = place.Longitude,
        IsFeatured = place.IsFeatured,
        DistrictName = place.District.Name,
    };

    private static HeritageFestivalSummaryDto ToFestivalSummary(Domain.Entities.HeritageDiscovery.HeritageFestival festival) => new()
    {
        Id = festival.Id,
        Name = festival.Name,
        Description = festival.Description,
        StartDate = festival.StartDate,
        EndDate = festival.EndDate,
        DistrictName = festival.District.Name,
    };

    private static CulturalEventSummaryDto ToEventSummary(Domain.Entities.HeritageDiscovery.CulturalEvent culturalEvent) => new()
    {
        Id = culturalEvent.Id,
        Name = culturalEvent.Name,
        Description = culturalEvent.Description,
        Category = culturalEvent.Category,
        EventDate = culturalEvent.EventDate,
        DistrictName = culturalEvent.District.Name,
    };

    private static LocalCuisineSummaryDto ToCuisineSummary(Domain.Entities.HeritageDiscovery.LocalCuisine cuisine) => new()
    {
        Id = cuisine.Id,
        Name = cuisine.Name,
        Description = cuisine.Description,
        WhereToTry = cuisine.WhereToTry,
        DistrictName = cuisine.District.Name,
    };

    private static TouristServiceSummaryDto ToServiceSummary(Domain.Entities.TouristBooking.TouristService service) => new()
    {
        Id = service.Id,
        Title = service.Title,
        Type = service.Type.ToString(),
        Price = service.Price,
        DurationMinutes = service.DurationMinutes,
        DistrictName = service.District.Name,
        Latitude = service.Latitude,
        Longitude = service.Longitude,
    };

    private static TourismLocationSummaryDto ToTourismLocationSummary(Domain.Entities.Tourism.TourismLocation location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        Type = location.Type.ToString(),
        Price = location.Price,
        EntryFee = location.EntryFee,
        IsVerified = location.IsVerified,
        VerificationStatus = location.VerificationStatus,
        Area = location.Area,
        CoordinatesPrecision = location.CoordinatesPrecision,
        OpeningHours = location.OpeningHours,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
        DistrictName = location.District.Name,
    };
}
