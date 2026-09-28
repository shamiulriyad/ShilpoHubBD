// Regression checks for the real standalone Tourism Budget Planner (AITourismService.PlanBudgetAsync).
// Exercises the actual AITourismService orchestration -- real TouristService prices from the
// repository, real TransportOption fares, and the real DummyAITourismProvider for the deterministic
// line-item math (GeminiAITourismProvider.PlanBudgetAsync always delegates to this same fallback --
// budget math never calls Gemini) -- against in-memory fakes for every other dependency. No database,
// no HTTP client, no network calls anywhere in this file. Run with `dotnet run`.
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

var dhaka = new District { Id = Guid.NewGuid(), Name = "Dhaka", Division = "Dhaka" };
var chattogram = new District { Id = Guid.NewGuid(), Name = "Chattogram", Division = "Chattogram" };

TouristService MakeService(District district, string title, BookingType type, decimal price) => new()
{
    Id = Guid.NewGuid(),
    Title = title,
    Type = type,
    Price = price,
    DistrictId = district.Id,
    District = district,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
};

TransportOption MakeOption(string origin, District destination, string op, decimal? fare, string verification = "Verified") => new()
{
    Id = Guid.NewGuid(),
    Mode = "Bus",
    OriginName = origin,
    DestinationDistrict = destination.Name,
    Operator = op,
    ServiceName = $"{op} Express",
    FareBdt = fare,
    VerificationStatus = verification,
    SourceUrl = "https://example.test/source",
    DataRetrievedOn = DateTime.UtcNow,
    IsActive = true,
};

AITourismService MakeTourismService(List<TouristService> services, List<TransportOption> transportOptions) => new(
    aiTourismProvider: new DummyAITourismProvider(new FakeRoutingProvider()),
    geocodingProvider: new FakeGeocodingProvider(),
    routingProvider: new FakeRoutingProvider(),
    travelPlannerRagProvider: new FakeTravelPlannerRagProvider(),
    heritagePlaceRepository: new FakeHeritagePlaceRepository(),
    heritageFestivalRepository: new FakeHeritageFestivalRepository(),
    culturalEventRepository: new FakeCulturalEventRepository(),
    localCuisineRepository: new FakeLocalCuisineRepository(),
    touristServiceRepository: new FakeTouristServiceRepository(services),
    tourismLocationRepository: new FakeTourismLocationRepository(),
    districtRepository: new FakeDistrictRepository(),
    transportOptionRepository: new FakeTransportOptionRepository(transportOptions),
    budgetEstimateOptions: Options.Create(new BudgetEstimateOptions()));

// ===================== 1. Basic successful budget calculation =====================
{
    var guide = MakeService(dhaka, "City Heritage Guide Tour", BookingType.GuideBooking, 500m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption>());

    var result = await service.PlanBudgetAsync(
        new BudgetPlanRequest
        {
            Selections = new List<BudgetSelectionDto> { new() { ServiceId = guide.Id, PartySize = 2 } },
            DurationDays = 1,
            PartySize = 2,
        },
        CancellationToken.None);

    Check("Basic: one line item for the selected service", result.LineItems.Count == 1);
    Check("Basic: service line amount is the real price times the real party size (500 x 2)", result.LineItems[0].Amount == 1000m);
    Check("Basic: verified total equals the sum of verified line items", result.TotalEstimatedCost == 1000m);
    Check("Basic: per-person cost is the verified total split across the party", result.PerPersonCost == 500m);
    Check("Basic: notes confirm how many services are covered", result.Notes.Contains("1 selected service"));
}

// ===================== 2. Accommodation cost from real tourist-service data =====================
{
    var homestay = MakeService(dhaka, "Riverside Homestay", BookingType.HomestayBooking, 1500m);
    var service = MakeTourismService(new List<TouristService> { homestay }, new List<TransportOption>());

    var result = await service.PlanBudgetAsync(
        new BudgetPlanRequest
        {
            Selections = new List<BudgetSelectionDto> { new() { ServiceId = homestay.Id, PartySize = 3 } },
            DurationDays = 2,
            PartySize = 3,
        },
        CancellationToken.None);

    var accommodationLine = result.LineItems.FirstOrDefault(l => l.Category == nameof(BookingType.HomestayBooking));
    Check("Accommodation: a line item exists for the selected homestay", accommodationLine is not null);
    Check("Accommodation: its amount is exactly the real service price times party size, not a guessed figure",
        accommodationLine!.Amount == homestay.Price * 3);
    Check("Accommodation: its label names the real service", accommodationLine.Label == homestay.Title);
}

// ===================== 3. Transport cost uses the selected TransportOption fare where available =====================
{
    var guide = MakeService(chattogram, "Hill Tracks Guide", BookingType.GuideBooking, 400m);
    var cheapOption = MakeOption("Dhaka", chattogram, "Green Line", fare: 800m);
    var pricierOption = MakeOption("Dhaka", chattogram, "Shohagh", fare: 1200m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption> { pricierOption, cheapOption });

    var result = await service.PlanBudgetAsync(
        new BudgetPlanRequest
        {
            Selections = new List<BudgetSelectionDto> { new() { ServiceId = guide.Id, PartySize = 2 } },
            DurationDays = 1,
            PartySize = 2,
        },
        CancellationToken.None);

    var transportLine = result.LineItems.FirstOrDefault(l => l.Category == "Transport");
    Check("Transport: a line item is added when a fare is available", transportLine is not null);
    Check("Transport: the lowest reported fare is used, not the first or the highest", transportLine!.Label.Contains("Green Line"));
    Check("Transport: amount is round-trip fare x party size (800 x 2 travellers x 2 legs)", transportLine.Amount == 800m * 2 * 2);
    Check("Transport: a verified fare is not flagged as unverified", !transportLine.Label.Contains("unverified"));
    Check("Transport: the verified total includes the real transport line", result.TotalEstimatedCost == (guide.Price * 2) + transportLine.Amount);
}

// ===================== 3b. An unverified fare is honestly labelled, never presented as certain =====================
{
    var guide = MakeService(chattogram, "Hill Tracks Guide", BookingType.GuideBooking, 400m);
    var unverifiedOption = MakeOption("Dhaka", chattogram, "Local Operator", fare: 600m, verification: "SecondarySource");
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption> { unverifiedOption });

    var result = await service.PlanBudgetAsync(
        new BudgetPlanRequest
        {
            Selections = new List<BudgetSelectionDto> { new() { ServiceId = guide.Id, PartySize = 1 } },
            DurationDays = 1,
            PartySize = 1,
        },
        CancellationToken.None);

    var transportLine = result.LineItems.FirstOrDefault(l => l.Category == "Transport");
    Check("Unverified fare: still included in the total (a real reported figure)", transportLine is not null);
    Check("Unverified fare: the label honestly flags it as unverified", transportLine!.Label.Contains("unverified"));
    Check("Unverified fare: the notes explain the fare is not confirmed", result.Notes.Contains("unverified source"));
}

// ===================== 4. Multiple days affect only the per-diem components, never the verified prices =====================
{
    var guide = MakeService(dhaka, "City Heritage Guide Tour", BookingType.GuideBooking, 500m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption>());

    var oneDay = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 2 } }, DurationDays = 1, PartySize = 2 },
        CancellationToken.None);
    var fiveDays = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 2 } }, DurationDays = 5, PartySize = 2 },
        CancellationToken.None);

    Check("Duration: the verified service line item is unaffected by trip length", oneDay.LineItems[0].Amount == fiveDays.LineItems[0].Amount);
    Check("Duration: the verified total is unaffected by trip length", oneDay.TotalEstimatedCost == fiveDays.TotalEstimatedCost);

    var mealsOne = oneDay.EstimatedItems.First(i => i.Category == "Food").Amount;
    var mealsFive = fiveDays.EstimatedItems.First(i => i.Category == "Food").Amount;
    Check("Duration: the per-diem meal estimate scales exactly with the number of days (5x for 5 days)", mealsFive == mealsOne * 5);

    var miscOne = oneDay.EstimatedItems.First(i => i.Category == "Miscellaneous").Amount;
    var miscFive = fiveDays.EstimatedItems.First(i => i.Category == "Miscellaneous").Amount;
    Check("Duration: the per-diem misc estimate scales exactly with the number of days (5x for 5 days)", miscFive == miscOne * 5);
}

// ===================== 5. Estimated values stay explicitly marked as estimated, never folded into the verified total =====================
{
    var guide = MakeService(dhaka, "City Heritage Guide Tour", BookingType.GuideBooking, 500m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption>());

    var result = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 2 } }, DurationDays = 1, PartySize = 2 },
        CancellationToken.None);

    Check("Estimated: meals/misc are reported as EstimatedItems, not verified LineItems", result.LineItems.All(l => l.Category is not ("Food" or "Miscellaneous")));
    Check("Estimated: every estimated item's label says it is assumed", result.EstimatedItems.All(i => i.Label.Contains("assumed")));
    Check("Estimated: an estimate note is present and says so explicitly", !string.IsNullOrWhiteSpace(result.EstimateNote) && result.EstimateNote!.Contains("estimate"));
    Check("Estimated: the verified total excludes the per-diem estimate", result.TotalEstimatedCost == 1000m);
    Check("Estimated: the combined estimated total is the verified total plus the assumed items",
        result.EstimatedTotal == result.TotalEstimatedCost + result.EstimatedItems.Sum(i => i.Amount));
}

// ===================== 6. Missing/unknown price data is never silently turned into a fake exact price =====================
{
    // 6a: services span more than one district -- no single fare can be attributed.
    var guideDhaka = MakeService(dhaka, "Old Dhaka Walk", BookingType.GuideBooking, 300m);
    var guideChattogram = MakeService(chattogram, "Hill Tracks Guide", BookingType.GuideBooking, 400m);
    var multiDistrictService = MakeTourismService(
        new List<TouristService> { guideDhaka, guideChattogram },
        new List<TransportOption> { MakeOption("Dhaka", chattogram, "Green Line", fare: 800m) });

    var multiDistrictResult = await multiDistrictService.PlanBudgetAsync(
        new BudgetPlanRequest
        {
            Selections = new() { new() { ServiceId = guideDhaka.Id, PartySize = 1 }, new() { ServiceId = guideChattogram.Id, PartySize = 1 } },
            DurationDays = 1,
            PartySize = 1,
        },
        CancellationToken.None);

    Check("Missing price (multi-district): no fabricated Transport line item is added",
        multiDistrictResult.LineItems.All(l => l.Category != "Transport"));
    Check("Missing price (multi-district): the gap is named honestly in UnverifiedCosts instead",
        multiDistrictResult.UnverifiedCosts.Any(c => c.Contains("multiple districts")));

    // 6b: single district, but no TransportOption rows exist for it at all.
    var noDataService = MakeTourismService(new List<TouristService> { guideChattogram }, new List<TransportOption>());
    var noDataResult = await noDataService.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guideChattogram.Id, PartySize = 1 } }, DurationDays = 1, PartySize = 1 },
        CancellationToken.None);

    Check("Missing price (no data): no fabricated Transport line item is added", noDataResult.LineItems.All(l => l.Category != "Transport"));
    Check("Missing price (no data): the gap is named honestly in UnverifiedCosts instead",
        noDataResult.UnverifiedCosts.Any(c => c.Contains("no verified service and fare data")));
    Check("Missing price (no data): the verified total is exactly the real service price, never inflated by a guessed fare",
        noDataResult.TotalEstimatedCost == guideChattogram.Price);
}

// ===================== 7. Invalid input is handled correctly =====================
{
    var guide = MakeService(dhaka, "City Heritage Guide Tour", BookingType.GuideBooking, 500m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption>());

    // 7a: a selection referencing a service id that does not exist.
    var unknownId = Guid.NewGuid();
    var threwNotFound = false;
    try
    {
        await service.PlanBudgetAsync(
            new BudgetPlanRequest { Selections = new() { new() { ServiceId = unknownId, PartySize = 1 } }, DurationDays = 1, PartySize = 1 },
            CancellationToken.None);
    }
    catch (NotFoundException exc)
    {
        threwNotFound = exc.Message.Contains(unknownId.ToString());
    }
    Check("Invalid input: an unknown service id throws NotFoundException naming the id, not a silent empty result", threwNotFound);

    // 7b: no services selected at all -- a valid, honest "nothing selected yet" result.
    var emptyResult = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new(), DurationDays = 1, PartySize = 1 }, CancellationToken.None);
    Check("Invalid input: zero selections gives an honest empty result, not an error", emptyResult.LineItems.Count == 0 && emptyResult.TotalEstimatedCost == 0m);
    Check("Invalid input: zero selections is explained in the notes", emptyResult.Notes.Contains("No services selected"));

    // 7c: a non-positive party size and an out-of-range duration are clamped, never used raw.
    var clampedResult = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 0 } }, DurationDays = 0, PartySize = 0 },
        CancellationToken.None);
    Check("Invalid input: a zero party size is clamped up to 1, never used as a zero multiplier",
        clampedResult.LineItems[0].Amount == guide.Price * 1);
    Check("Invalid input: a zero duration is clamped up to 1 day of per-diem, never zero days",
        clampedResult.EstimatedItems.First(i => i.Category == "Food").Amount > 0m);

    var overlongResult = await service.PlanBudgetAsync(
        new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 1 } }, DurationDays = 500, PartySize = 1 },
        CancellationToken.None);
    var mealsAt90 = 600m * 1 * 90;
    Check("Invalid input: an out-of-range duration is clamped to the maximum (90 days), never used raw",
        overlongResult.EstimatedItems.First(i => i.Category == "Food").Amount == mealsAt90);
}

// ===================== 8. Deterministic math -- no Gemini/AI or network call is required =====================
{
    // The whole file never constructs an HttpClient or a Gemini-backed provider: PlanBudgetAsync is
    // driven end-to-end by DummyAITourismProvider (the same fallback GeminiAITourismProvider.PlanBudgetAsync
    // always delegates to). Determinism across repeated calls with identical input is the observable
    // proof that no external/AI call and no randomness is involved.
    var guide = MakeService(dhaka, "City Heritage Guide Tour", BookingType.GuideBooking, 500m);
    var service = MakeTourismService(new List<TouristService> { guide }, new List<TransportOption>());
    var request = new BudgetPlanRequest { Selections = new() { new() { ServiceId = guide.Id, PartySize = 2 } }, DurationDays = 2, PartySize = 2 };

    var first = await service.PlanBudgetAsync(request, CancellationToken.None);
    var second = await service.PlanBudgetAsync(request, CancellationToken.None);

    Check("Deterministic: identical input produces an identical verified total across repeated calls",
        first.TotalEstimatedCost == second.TotalEstimatedCost);
    Check("Deterministic: identical input produces an identical estimated total across repeated calls",
        first.EstimatedTotal == second.EstimatedTotal);
    Check("Deterministic: identical input produces the same number of line items across repeated calls",
        first.LineItems.Count == second.LineItems.Count);
}

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

// ===================== Fakes: the two repositories PlanBudgetAsync actually reads from =====================

class FakeTouristServiceRepository : ITouristServiceRepository
{
    private readonly List<TouristService> _services;
    public FakeTouristServiceRepository(List<TouristService> services) => _services = services;

    public Task<(List<TouristService> Items, int TotalCount)> GetPagedAsync(TouristServiceQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<(List<TouristService> Items, int TotalCount)> GetPagedByProducerAsync(Guid producerId, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<TouristService?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<TouristService>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => Task.FromResult(_services.Where(s => ids.Contains(s.Id)).ToList());
    public Task AddAsync(TouristService service, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(TouristService service) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeTransportOptionRepository : ITransportOptionRepository
{
    private readonly List<TransportOption> _options;
    public FakeTransportOptionRepository(List<TransportOption> options) => _options = options;

    public Task<List<TransportOption>> GetForDestinationAsync(string destinationDistrict, string mode, CancellationToken cancellationToken) =>
        Task.FromResult(_options.Where(o => o.DestinationDistrict == destinationDistrict && o.Mode == mode && o.IsActive).ToList());
}

// ===================== Fakes: every other AITourismService dependency, unused by PlanBudgetAsync =====================

class FakeGeocodingProvider : IGeocodingProvider
{
    public Task<GeoPointDto?> GeocodeAsync(string query, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeRoutingProvider : IRoutingProvider
{
    public Task<RouteResultDto?> GetDrivingRouteAsync(GeoPointDto origin, GeoPointDto destination, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeTravelPlannerRagProvider : ITravelPlannerRagProvider
{
    public Task<List<RagTravelNoteDto>> RetrieveAsync(string query, string? district, IReadOnlyList<string>? interests, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<DistrictDatasetResult> GetDistrictEntitiesAsync(string district, IReadOnlyList<string>? interests, int limit, CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeHeritagePlaceRepository : IHeritagePlaceRepository
{
    public Task<(List<HeritagePlace> Items, int TotalCount)> GetPagedAsync(HeritagePlaceQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<HeritagePlace>> GetActiveWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<HeritagePlace?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<HeritagePlace>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(HeritagePlace place, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(HeritagePlace place) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeHeritageFestivalRepository : IHeritageFestivalRepository
{
    public Task<(List<HeritageFestival> Items, int TotalCount)> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<HeritageFestival?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(HeritageFestival festival, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(HeritageFestival festival) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeCulturalEventRepository : ICulturalEventRepository
{
    public Task<(List<CulturalEvent> Items, int TotalCount)> GetPagedAsync(CulturalEventQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<CulturalEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(CulturalEvent culturalEvent, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(CulturalEvent culturalEvent) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeLocalCuisineRepository : ILocalCuisineRepository
{
    public Task<(List<LocalCuisine> Items, int TotalCount)> GetPagedAsync(LocalCuisineQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<LocalCuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task AddAsync(LocalCuisine cuisine, CancellationToken cancellationToken) => throw new NotImplementedException();
    public void Remove(LocalCuisine cuisine) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

class FakeTourismLocationRepository : ITourismLocationRepository
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

class FakeDistrictRepository : IDistrictRepository
{
    public Task<List<District>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
