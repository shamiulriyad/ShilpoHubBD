using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Services.HeritageDiscovery;

public class HeritageRouteService : IHeritageRouteService
{
    private const double EarthRadiusKm = 6371.0;

    private static readonly Dictionary<TransportationMode, double> AverageSpeedKmh = new()
    {
        [TransportationMode.Walking] = 4,
        [TransportationMode.Bike] = 15,
        [TransportationMode.Car] = 40,
        [TransportationMode.Bus] = 30,
        [TransportationMode.Boat] = 20,
        [TransportationMode.Mixed] = 25,
    };

    private readonly IHeritageRouteRepository _routeRepository;
    private readonly IHeritagePlaceRepository _placeRepository;

    public HeritageRouteService(IHeritageRouteRepository routeRepository, IHeritagePlaceRepository placeRepository)
    {
        _routeRepository = routeRepository;
        _placeRepository = placeRepository;
    }

    public async Task<PagedResult<HeritageRouteDto>> GetPagedAsync(HeritageRouteQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _routeRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<HeritageRouteDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<List<HeritageRouteDto>> GetRecommendedAsync(CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetRecommendedAsync(cancellationToken);
        return routes.Select(ToDto).ToList();
    }

    public async Task<HeritageRouteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");
        return ToDto(route);
    }

    public async Task<HeritageRouteDto> CreateAsync(CreateHeritageRouteRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var route = new HeritageRoute
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            IsRecommended = request.IsRecommended,
            Status = RouteStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _routeRepository.AddAsync(route, cancellationToken);
        await _routeRepository.SaveChangesAsync(cancellationToken);

        return ToDto(route);
    }

    public async Task<HeritageRouteDto> UpdateAsync(Guid id, UpdateHeritageRouteRequest request, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");

        route.Name = request.Name.Trim();
        route.Description = request.Description.Trim();
        route.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        route.IsRecommended = request.IsRecommended;
        route.Status = request.Status;
        route.UpdatedAt = DateTime.UtcNow;

        await _routeRepository.SaveChangesAsync(cancellationToken);

        return ToDto(route);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");

        _routeRepository.Remove(route);
        await _routeRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<HeritageRouteDto> AddStopAsync(Guid routeId, CreateRouteStopRequest request, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");

        var place = await _placeRepository.GetByIdAsync(request.HeritagePlaceId, cancellationToken)
            ?? throw new NotFoundException("Heritage place not found.");

        if (route.Stops.Any(s => s.HeritagePlaceId == place.Id))
        {
            throw new ConflictException("This heritage place is already a stop on this route.");
        }

        var stop = new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = route.Id,
            HeritagePlaceId = place.Id,
            HeritagePlace = place,
            TransportationMode = request.TransportationMode,
            Notes = request.Notes?.Trim(),
        };

        route.Stops.Add(stop);
        await _routeRepository.AddStopAsync(stop, cancellationToken);

        ApplySequence(route, route.Stops.OrderBy(s => s.Order).ToList());
        route.UpdatedAt = DateTime.UtcNow;

        await _routeRepository.SaveChangesAsync(cancellationToken);

        return ToDto(route);
    }

    public async Task<HeritageRouteDto> RemoveStopAsync(Guid routeId, Guid stopId, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId)
            ?? throw new NotFoundException("Route stop not found.");

        route.Stops.Remove(stop);
        _routeRepository.RemoveStop(stop);

        ApplySequence(route, route.Stops.OrderBy(s => s.Order).ToList());
        route.UpdatedAt = DateTime.UtcNow;

        await _routeRepository.SaveChangesAsync(cancellationToken);

        return ToDto(route);
    }

    public async Task<HeritageRouteDto> ReorderStopsAsync(Guid routeId, ReorderStopsRequest request, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, cancellationToken)
            ?? throw new NotFoundException("Heritage route not found.");

        var currentIds = route.Stops.Select(s => s.Id).ToHashSet();
        if (request.StopIds.Count != currentIds.Count || !request.StopIds.All(currentIds.Contains))
        {
            throw new ConflictException("StopIds must match the route's existing stops exactly.");
        }

        // Two-phase update: move every stop to a temporary, non-colliding Order first, since the
        // (RouteId, Order) unique index is checked per-statement and a direct permutation can collide mid-transaction.
        var orderedStops = request.StopIds.Select(id => route.Stops.First(s => s.Id == id)).ToList();
        for (var i = 0; i < orderedStops.Count; i++)
        {
            orderedStops[i].Order = -(i + 1);
        }
        await _routeRepository.SaveChangesAsync(cancellationToken);

        ApplySequence(route, orderedStops);
        route.UpdatedAt = DateTime.UtcNow;
        await _routeRepository.SaveChangesAsync(cancellationToken);

        return ToDto(route);
    }

    private static void ApplySequence(HeritageRoute route, List<RouteStop> orderedStops)
    {
        double total = 0;

        for (var i = 0; i < orderedStops.Count; i++)
        {
            var stop = orderedStops[i];
            stop.Order = i + 1;

            if (i == 0)
            {
                stop.DistanceFromPreviousKm = null;
                stop.EstimatedTravelMinutesFromPrevious = null;
                continue;
            }

            var previous = orderedStops[i - 1];
            var distanceKm = HaversineDistanceKm(
                previous.HeritagePlace.Latitude, previous.HeritagePlace.Longitude,
                stop.HeritagePlace.Latitude, stop.HeritagePlace.Longitude);

            stop.DistanceFromPreviousKm = Math.Round(distanceKm, 2);
            stop.EstimatedTravelMinutesFromPrevious = EstimateMinutes(distanceKm, stop.TransportationMode);
            total += distanceKm;
        }

        route.TotalDistanceKm = Math.Round(total, 2);
    }

    private static int EstimateMinutes(double distanceKm, TransportationMode mode)
    {
        if (distanceKm <= 0)
        {
            return 0;
        }

        var speedKmh = AverageSpeedKmh[mode];
        return Math.Max(1, (int)Math.Round(distanceKm / speedKmh * 60));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static HeritageRouteDto ToDto(HeritageRoute route) => new()
    {
        Id = route.Id,
        Name = route.Name,
        Description = route.Description,
        EstimatedDurationMinutes = route.EstimatedDurationMinutes,
        TotalDistanceKm = route.TotalDistanceKm,
        IsRecommended = route.IsRecommended,
        Status = route.Status.ToString(),
        Stops = route.Stops.OrderBy(s => s.Order).Select(ToStopDto).ToList(),
        CreatedAt = route.CreatedAt,
        UpdatedAt = route.UpdatedAt,
    };

    private static RouteStopDto ToStopDto(RouteStop stop) => new()
    {
        Id = stop.Id,
        HeritagePlaceId = stop.HeritagePlaceId,
        HeritagePlaceName = stop.HeritagePlace.Name,
        Latitude = stop.HeritagePlace.Latitude,
        Longitude = stop.HeritagePlace.Longitude,
        Order = stop.Order,
        DistanceFromPreviousKm = stop.DistanceFromPreviousKm,
        EstimatedTravelMinutesFromPrevious = stop.EstimatedTravelMinutesFromPrevious,
        TransportationMode = stop.TransportationMode.ToString(),
        Notes = stop.Notes,
    };
}
