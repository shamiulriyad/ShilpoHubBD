// Regression checks for the real route-optimization logic, at two layers:
//
// 1. DummyAITourismProvider.OptimizeRouteAsync directly: real road distance + travel time are surfaced
//    when routing succeeds, a genuinely unavailable routing engine falls back to real (Haversine)
//    geometry with an honestly-null travel time rather than a guessed number, a mixed route never
//    reports a partial duration as if it were the whole trip's time, and the origin is always echoed
//    back. No network calls -- a fake IRoutingProvider stands in for OSRM.
//
// 2. AITourismService.OptimizeRouteAsync itself (the service-layer orchestration that previously had no
//    dedicated coverage): real HeritagePlace ids resolve to real coordinates and are what the provider
//    actually receives; an unknown place id throws NotFoundException before any provider/route call;
//    invalid (0,0) or out-of-range coordinates are filtered out before reaching the provider and are
//    honestly reported as excluded, never silently dropped or sent to routing; an invalid start point is
//    discarded (not forwarded) with an honest note, never used to fabricate a route; and a provider
//    failure propagates instead of being swallowed into a fake successful result. A recording fake
//    IAITourismProvider stands in for the AI layer so each test can both control the response and
//    inspect exactly what context the service handed it -- no network/Gemini/OSRM calls anywhere here.
//
// Run with `dotnet run`.
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Application.Services.AITourism;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.TouristBooking;
using ShilpoHubBD.Domain.Entities.Tourism;
using ShilpoHubBD.Infrastructure.AITourism;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

var placeA = new RoutePlaceDto { Id = Guid.NewGuid(), Name = "Place A", Latitude = 23.81, Longitude = 90.41 };
var placeB = new RoutePlaceDto { Id = Guid.NewGuid(), Name = "Place B", Latitude = 23.82, Longitude = 90.42 };
var placeC = new RoutePlaceDto { Id = Guid.NewGuid(), Name = "Place C", Latitude = 23.83, Longitude = 90.43 };

// ===================== All legs have real road routing =====================
var providerAllReal = new DummyAITourismProvider(new FakeRoutingProvider((_, _) => new RouteResultDto { DistanceKm = 5.0, DurationMinutes = 12 }));
var resultAllReal = await providerAllReal.OptimizeRouteAsync(
    new RouteOptimizationContext { Places = new List<RoutePlaceDto> { placeA, placeB, placeC } }, CancellationToken.None);

Check("All legs real: TotalEstimatedTravelMinutes is set from real routing", resultAllReal.TotalEstimatedTravelMinutes.HasValue);
Check("All legs real: every stop carries a real travel time", resultAllReal.Stops.All(s => s.EstimatedTravelMinutesFromPrevious.HasValue));
Check("All legs real: notes say real road distances and travel times were used", resultAllReal.Notes.Contains("real road distances and travel times"));
Check("No explicit start point: origin description names the first selected place", resultAllReal.OriginDescription.Contains(placeA.Name));

// ===================== Routing engine unavailable for every leg =====================
var providerAllFallback = new DummyAITourismProvider(new FakeRoutingProvider((_, _) => null));
var resultAllFallback = await providerAllFallback.OptimizeRouteAsync(
    new RouteOptimizationContext { Places = new List<RoutePlaceDto> { placeA, placeB, placeC } }, CancellationToken.None);

Check("Routing unavailable: TotalEstimatedTravelMinutes stays null, never a guessed number", resultAllFallback.TotalEstimatedTravelMinutes is null);
Check("Routing unavailable: no stop reports a travel time it doesn't actually have", resultAllFallback.Stops.All(s => s.EstimatedTravelMinutesFromPrevious is null));
Check("Routing unavailable: distance is still real geometry, not zero or fabricated", resultAllFallback.TotalDistanceKm > 0);
Check("Routing unavailable: notes are honest about the fallback", resultAllFallback.Notes.Contains("road routing unavailable"));

// ===================== Mixed: routing available for some legs, not others =====================
var providerMixed = new DummyAITourismProvider(new FakeRoutingProvider(
    (origin, _) => origin.Latitude == placeA.Latitude ? new RouteResultDto { DistanceKm = 5.0, DurationMinutes = 10 } : null));
var resultMixed = await providerMixed.OptimizeRouteAsync(
    new RouteOptimizationContext { Places = new List<RoutePlaceDto> { placeA, placeB, placeC } }, CancellationToken.None);

Check("Mixed availability: total travel time is never reported as a partial-but-labelled-complete number",
    resultMixed.TotalEstimatedTravelMinutes is null);
Check("Mixed availability: the leg that did have real routing still reports its own real travel time",
    resultMixed.Stops.Any(s => s.EstimatedTravelMinutesFromPrevious.HasValue));

// ===================== Custom start point is echoed back honestly =====================
var providerWithStart = new DummyAITourismProvider(new FakeRoutingProvider((_, _) => new RouteResultDto { DistanceKm = 3.0, DurationMinutes = 8 }));
var resultWithStart = await providerWithStart.OptimizeRouteAsync(
    new RouteOptimizationContext { Places = new List<RoutePlaceDto> { placeA, placeB }, StartLatitude = 23.80, StartLongitude = 90.40 },
    CancellationToken.None);

Check("Custom start point: origin description reflects it", resultWithStart.OriginDescription == "Custom starting point");
Check("Custom start point: origin coordinates echo exactly what was requested",
    resultWithStart.OriginLatitude == 23.80 && resultWithStart.OriginLongitude == 90.40);

// ===================== No places at all =====================
var resultEmpty = await providerAllFallback.OptimizeRouteAsync(new RouteOptimizationContext { Places = new List<RoutePlaceDto>() }, CancellationToken.None);
Check("No places provided: an honest empty result, not an error or a fabricated route", resultEmpty.Stops.Count == 0 && resultEmpty.TotalDistanceKm == 0);

// ============================================================================================
// Service-layer tests: AITourismService.OptimizeRouteAsync itself, not just the provider below it.
// ============================================================================================

var district = new District { Id = Guid.NewGuid(), Name = "Dhaka", Division = "Dhaka" };
HeritagePlace MakePlace(string name, double lat, double lng) => new()
{
    Id = Guid.NewGuid(), Name = name, Latitude = lat, Longitude = lng,
    DistrictId = district.Id, District = district, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};

var servicePlaceA = MakePlace("Ahsan Manzil", 23.7086, 90.4056);
var servicePlaceB = MakePlace("Lalbagh Fort", 23.7189, 90.3883);
var serviceInvalidPlace = MakePlace("Never Geocoded Place", 0, 0);
var serviceOutOfRangePlace = MakePlace("Corrupt Coordinates Place", 200, 90.41);

AITourismService MakeTourismService(IAITourismProvider provider, List<HeritagePlace> places) => new(
    aiTourismProvider: provider,
    geocodingProvider: new UnusedGeocodingProvider(),
    routingProvider: new UnusedRoutingProvider(),
    travelPlannerRagProvider: new UnusedTravelPlannerRagProvider(),
    heritagePlaceRepository: new FakeHeritagePlaceRepository(places),
    heritageFestivalRepository: new UnusedHeritageFestivalRepository(),
    culturalEventRepository: new UnusedCulturalEventRepository(),
    localCuisineRepository: new UnusedLocalCuisineRepository(),
    touristServiceRepository: new UnusedTouristServiceRepository(),
    tourismLocationRepository: new UnusedTourismLocationRepository(),
    districtRepository: new UnusedDistrictRepository(),
    transportOptionRepository: new UnusedTransportOptionRepository(),
    budgetEstimateOptions: Options.Create(new BudgetEstimateOptions()));

RouteOptimizationResult CannedResult(RouteOptimizationContext ctx) => new()
{
    Stops = ctx.Places.Select((p, i) => new OptimizedStopDto { PlaceId = p.Id, Name = p.Name, Order = i + 1 }).ToList(),
    TotalDistanceKm = 1,
    OriginLatitude = ctx.StartLatitude,
    OriginLongitude = ctx.StartLongitude,
};

// ===================== 1. Valid place ids: resolved correctly, provider called with valid coordinates =====================
{
    var provider = new RecordingAITourismProvider(CannedResult);
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA, servicePlaceB });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id, servicePlaceB.Id } }, CancellationToken.None);

    Check("Valid place ids: the provider receives exactly the two real, resolved places",
        provider.LastContext!.Places.Count == 2);
    Check("Valid place ids: the provider receives the place's real database coordinates, not invented ones",
        provider.LastContext.Places.Any(p => p.Id == servicePlaceA.Id && p.Latitude == servicePlaceA.Latitude && p.Longitude == servicePlaceA.Longitude)
        && provider.LastContext.Places.Any(p => p.Id == servicePlaceB.Id && p.Latitude == servicePlaceB.Latitude && p.Longitude == servicePlaceB.Longitude));
    Check("Valid place ids: nothing is excluded", result.ExcludedPlaces.Count == 0);
    Check("Valid place ids: the provider's real result is returned", result.Stops.Count == 2);
}

// ===================== 2. Missing/unknown place id: NotFoundException, no fake route =====================
{
    var provider = new RecordingAITourismProvider(_ => throw new InvalidOperationException(
        "the provider must never be called when a requested place id does not exist"));
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA });
    var unknownId = Guid.NewGuid();
    var threwNotFound = false;
    var message = string.Empty;
    try
    {
        await service.OptimizeRouteAsync(
            new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id, unknownId } }, CancellationToken.None);
    }
    catch (NotFoundException exc)
    {
        threwNotFound = true;
        message = exc.Message;
    }
    Check("Missing place id: NotFoundException is thrown instead of a route being fabricated", threwNotFound);
    Check("Missing place id: the exception names the specific missing id", message.Contains(unknownId.ToString()));
}

// ===================== 3. Invalid/missing coordinates: excluded, never sent to the provider =====================
{
    var provider = new RecordingAITourismProvider(CannedResult);
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA, serviceInvalidPlace, serviceOutOfRangePlace });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id, serviceInvalidPlace.Id, serviceOutOfRangePlace.Id } },
        CancellationToken.None);

    Check("Invalid coordinates: the (0,0) never-geocoded place is not sent to the provider/routing",
        provider.LastContext!.Places.All(p => p.Id != serviceInvalidPlace.Id));
    Check("Invalid coordinates: the out-of-range place is not sent to the provider/routing",
        provider.LastContext.Places.All(p => p.Id != serviceOutOfRangePlace.Id));
    Check("Invalid coordinates: only the genuinely valid place reaches the provider",
        provider.LastContext.Places.Count == 1 && provider.LastContext.Places[0].Id == servicePlaceA.Id);
    Check("Invalid coordinates: both invalid places are honestly reported as excluded, not silently dropped",
        result.ExcludedPlaces.Contains(serviceInvalidPlace.Name) && result.ExcludedPlaces.Contains(serviceOutOfRangePlace.Name));
    Check("Invalid coordinates: the response explains why they were excluded",
        result.Notes.Contains("no verified coordinates"));
}

// ===================== 3b. Every place invalid: honest empty result, provider never called =====================
{
    var provider = new RecordingAITourismProvider(_ => throw new InvalidOperationException(
        "the provider must never be called when no place has valid coordinates"));
    var service = MakeTourismService(provider, new List<HeritagePlace> { serviceInvalidPlace, serviceOutOfRangePlace });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { serviceInvalidPlace.Id, serviceOutOfRangePlace.Id } }, CancellationToken.None);

    Check("All places invalid: an honest empty result, not an error or a fabricated route",
        result.Stops.Count == 0 && result.TotalDistanceKm == 0);
    Check("All places invalid: both are named as excluded",
        result.ExcludedPlaces.Contains(serviceInvalidPlace.Name) && result.ExcludedPlaces.Contains(serviceOutOfRangePlace.Name));
}

// ===================== 4. Invalid start point: discarded honestly, never used to fabricate a route =====================
{
    var provider = new RecordingAITourismProvider(CannedResult);
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id }, StartLatitude = 999, StartLongitude = 90.41 },
        CancellationToken.None);

    Check("Invalid start point: the bad coordinates are never forwarded to the provider",
        provider.LastContext!.StartLatitude is null && provider.LastContext.StartLongitude is null);
    Check("Invalid start point: a real result is still returned, not an error and not a route built from the bad point",
        result.Stops.Count == 1);
    Check("Invalid start point: the response honestly says the starting point was ignored",
        result.Notes.Contains("starting coordinates looked invalid"));
}

// ===================== 5. Valid start + valid places: the real route result is returned =====================
{
    var provider = new RecordingAITourismProvider(CannedResult);
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA, servicePlaceB });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id, servicePlaceB.Id }, StartLatitude = 23.80, StartLongitude = 90.40 },
        CancellationToken.None);

    Check("Valid start + valid places: the real start point reaches the provider unchanged",
        provider.LastContext!.StartLatitude == 23.80 && provider.LastContext.StartLongitude == 90.40);
    Check("Valid start + valid places: a real route result is returned",
        result.Stops.Count == 2 && result.OriginLatitude == 23.80 && result.OriginLongitude == 90.40);
}

// ===================== 6. Multiple stops: valid ones preserved, invalid-coordinate ones excluded =====================
{
    var servicePlaceC = MakePlace("Star Mosque", 23.7275, 90.4088);
    var provider = new RecordingAITourismProvider(CannedResult);
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA, servicePlaceB, servicePlaceC, serviceInvalidPlace });
    var result = await service.OptimizeRouteAsync(
        new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id, servicePlaceB.Id, servicePlaceC.Id, serviceInvalidPlace.Id } },
        CancellationToken.None);

    Check("Multiple stops: all three valid places are preserved and reach the provider",
        provider.LastContext!.Places.Count == 3);
    Check("Multiple stops: the invalid-coordinate stop never reaches the provider",
        provider.LastContext.Places.All(p => p.Id != serviceInvalidPlace.Id));
    Check("Multiple stops: the invalid stop is still honestly reported as excluded",
        result.ExcludedPlaces.Contains(serviceInvalidPlace.Name));
    Check("Multiple stops: the real provider result for the valid stops comes through unchanged",
        result.Stops.Count == 3);
}

// ===================== 7. Provider failure: the service never fabricates a successful result =====================
{
    var provider = new RecordingAITourismProvider(_ => throw new InvalidOperationException("simulated provider/OSRM failure"));
    var service = MakeTourismService(provider, new List<HeritagePlace> { servicePlaceA });
    var threw = false;
    try
    {
        await service.OptimizeRouteAsync(new RouteOptimizationRequest { PlaceIds = new List<Guid> { servicePlaceA.Id } }, CancellationToken.None);
    }
    catch (InvalidOperationException)
    {
        threw = true;
    }
    Check("Provider failure: the failure propagates instead of being swallowed into a fabricated successful route", threw);
}

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class FakeRoutingProvider : IRoutingProvider
{
    private readonly Func<GeoPointDto, GeoPointDto, RouteResultDto?> _responder;
    public FakeRoutingProvider(Func<GeoPointDto, GeoPointDto, RouteResultDto?> responder) => _responder = responder;

    public Task<RouteResultDto?> GetDrivingRouteAsync(GeoPointDto origin, GeoPointDto destination, CancellationToken cancellationToken)
        => Task.FromResult(_responder(origin, destination));
}

// ===================== Service-layer fakes: AITourismService.OptimizeRouteAsync's real dependencies =====================

// Records the exact RouteOptimizationContext AITourismService hands it, so a test can assert on
// precisely which places/coordinates survived the service's own filtering -- and can be made to throw,
// to prove a provider failure is never turned into a fabricated success.
class RecordingAITourismProvider : IAITourismProvider
{
    private readonly Func<RouteOptimizationContext, RouteOptimizationResult> _responder;
    public RouteOptimizationContext? LastContext { get; private set; }
    public RecordingAITourismProvider(Func<RouteOptimizationContext, RouteOptimizationResult> responder) => _responder = responder;

    public Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationContext context, CancellationToken cancellationToken)
    {
        LastContext = context;
        return Task.FromResult(_responder(context));
    }

    public Task<TourPlanResult> PlanTourAsync(TourPlanContext context, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanContext context, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationContext context, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeHeritagePlaceRepository : IHeritagePlaceRepository
{
    private readonly List<HeritagePlace> _places;
    public FakeHeritagePlaceRepository(List<HeritagePlace> places) => _places = places;

    public Task<(List<HeritagePlace> Items, int TotalCount)> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<HeritagePlace>> GetActiveWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<HeritagePlace?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<HeritagePlace>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => Task.FromResult(_places.Where(p => ids.Contains(p.Id)).ToList());
    public Task AddAsync(HeritagePlace place, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(HeritagePlace place) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

// Every other AITourismService dependency, untouched by OptimizeRouteAsync -- each throws if it is ever
// called, so an accidental call from a future change to the service would fail these tests loudly.
class UnusedGeocodingProvider : IGeocodingProvider
{
    public Task<GeoPointDto?> GeocodeAsync(string query, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedRoutingProvider : IRoutingProvider
{
    public Task<RouteResultDto?> GetDrivingRouteAsync(GeoPointDto origin, GeoPointDto destination, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedTravelPlannerRagProvider : ITravelPlannerRagProvider
{
    public Task<List<RagTravelNoteDto>> RetrieveAsync(string query, string? district, IReadOnlyList<string>? interests, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<DistrictDatasetResult> GetDistrictEntitiesAsync(string district, IReadOnlyList<string>? interests, int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedHeritageFestivalRepository : IHeritageFestivalRepository
{
    public Task<(List<HeritageFestival> Items, int TotalCount)> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<HeritageFestival?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(HeritageFestival festival, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(HeritageFestival festival) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedCulturalEventRepository : ICulturalEventRepository
{
    public Task<(List<CulturalEvent> Items, int TotalCount)> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<CulturalEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(CulturalEvent culturalEvent, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(CulturalEvent culturalEvent) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedLocalCuisineRepository : ILocalCuisineRepository
{
    public Task<(List<LocalCuisine> Items, int TotalCount)> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<LocalCuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(LocalCuisine cuisine, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(LocalCuisine cuisine) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedTouristServiceRepository : ITouristServiceRepository
{
    public Task<(List<TouristService> Items, int TotalCount)> GetPagedAsync(TouristServiceQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(List<TouristService> Items, int TotalCount)> GetPagedByProducerAsync(Guid producerId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<TouristService?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TouristService>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(TouristService service, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(TouristService service) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedTourismLocationRepository : ITourismLocationRepository
{
    public Task<(List<TourismLocation> Items, int TotalCount)> GetPagedAsync(TourismLocationQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<TourismLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TourismLocation>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TourismLocation>> GetForDistrictAsync(Guid districtId, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TourismLocation>> GetInBoundsAsync(double minLat, double maxLat, double minLon, double maxLon, IReadOnlyCollection<TourismLocationType> types, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TourismLocation>> GetAllForDistrictTrackedAsync(Guid districtId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<DateTime?> GetLastSyncedAtAsync(Guid districtId, IReadOnlyCollection<TourismLocationType>? types, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(TourismLocation location, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(TourismLocation location) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedDistrictRepository : IDistrictRepository
{
    public Task<List<District>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class UnusedTransportOptionRepository : ITransportOptionRepository
{
    public Task<List<TransportOption>> GetForDestinationAsync(string destinationDistrict, string mode, CancellationToken cancellationToken) => throw new NotImplementedException();
}
