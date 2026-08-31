using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AITourism;

public class AITourismService : IAITourismService
{
    private const decimal DefaultDailyFoodBudgetPerPerson = 600m;
    private const decimal DefaultDailyMiscBudgetPerPerson = 300m;

    private readonly IAITourismProvider _aiTourismProvider;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;
    private readonly IHeritageFestivalRepository _heritageFestivalRepository;
    private readonly ICulturalEventRepository _culturalEventRepository;
    private readonly ILocalCuisineRepository _localCuisineRepository;
    private readonly ITouristServiceRepository _touristServiceRepository;
    private readonly IDistrictRepository _districtRepository;

    public AITourismService(
        IAITourismProvider aiTourismProvider,
        IHeritagePlaceRepository heritagePlaceRepository,
        IHeritageFestivalRepository heritageFestivalRepository,
        ICulturalEventRepository culturalEventRepository,
        ILocalCuisineRepository localCuisineRepository,
        ITouristServiceRepository touristServiceRepository,
        IDistrictRepository districtRepository)
    {
        _aiTourismProvider = aiTourismProvider;
        _heritagePlaceRepository = heritagePlaceRepository;
        _heritageFestivalRepository = heritageFestivalRepository;
        _culturalEventRepository = culturalEventRepository;
        _localCuisineRepository = localCuisineRepository;
        _touristServiceRepository = touristServiceRepository;
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

        var context = new TourPlanContext
        {
            DistrictName = districtName,
            DurationDays = Math.Clamp(request.DurationDays, 1, 30),
            PartySize = Math.Max(1, request.PartySize),
            StartDate = request.StartDate,
            Places = places.Select(ToPlaceSummary).ToList(),
            Festivals = festivals.Select(ToFestivalSummary).ToList(),
            Events = events.Select(ToEventSummary).ToList(),
            Services = services.Select(ToServiceSummary).ToList(),
        };

        return await _aiTourismProvider.PlanTourAsync(context, cancellationToken);
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
    };
}
