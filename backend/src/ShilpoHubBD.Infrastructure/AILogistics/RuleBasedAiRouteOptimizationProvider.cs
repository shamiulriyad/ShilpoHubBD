using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Infrastructure.AILogistics;

/// <summary>
/// Rule-based stand-in for an AI route solver. Seeds a tour with nearest-neighbour from the route
/// start, then runs a bounded 2-opt improvement pass over haversine distances. Stops without
/// coordinates keep their original relative order and are appended. No external calls.
/// </summary>
public class RuleBasedAiRouteOptimizationProvider : IAiRouteOptimizationProvider
{
    public const string Name = "rule-based-route-2opt-v1";

    private const double EarthRadiusKm = 6371.0;
    private const int MaxTwoOptPasses = 12;

    public string ProviderName => Name;

    public AiRouteOptimizationResult Optimize(AiRouteOptimizationInput input)
    {
        var speed = input.AverageSpeedKmh is > 0 ? input.AverageSpeedKmh : 25.0;

        var located = input.Stops
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
            .OrderBy(s => s.OriginalSequence)
            .ToList();
        var unlocated = input.Stops
            .Where(s => !s.Latitude.HasValue || !s.Longitude.HasValue)
            .OrderBy(s => s.OriginalSequence)
            .ToList();

        var originalOrder = input.Stops.OrderBy(s => s.OriginalSequence).ToList();
        var originalDistance = PathDistance(input.StartLatitude, input.StartLongitude, originalOrder);

        var optimised = NearestNeighbour(input.StartLatitude, input.StartLongitude, located);
        TwoOpt(input.StartLatitude, input.StartLongitude, optimised);
        optimised.AddRange(unlocated);

        var proposedDistance = PathDistance(input.StartLatitude, input.StartLongitude, optimised);

        double totalMinutes = 0;
        double? prevLat = input.StartLatitude;
        double? prevLon = input.StartLongitude;
        var ordered = new List<AiRouteStopResult>(optimised.Count);
        for (var i = 0; i < optimised.Count; i++)
        {
            var stop = optimised[i];
            double? leg = null;
            if (prevLat.HasValue && prevLon.HasValue && stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                leg = Haversine(prevLat.Value, prevLon.Value, stop.Latitude.Value, stop.Longitude.Value);
                totalMinutes += leg.Value / speed * 60.0;
            }

            totalMinutes += stop.ServiceMinutes;
            ordered.Add(new AiRouteStopResult(
                stop.StopId, stop.OriginalSequence, i + 1,
                leg.HasValue ? Math.Round(leg.Value, 2) : (double?)null,
                stop.Label));

            if (stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                prevLat = stop.Latitude;
                prevLon = stop.Longitude;
            }
        }

        var saving = originalDistance - proposedDistance;
        var confidence = located.Count switch
        {
            >= 8 => AiLogisticsConfidence.High,
            >= 4 => AiLogisticsConfidence.Moderate,
            _ => AiLogisticsConfidence.Low,
        };

        var summary = located.Count < 2
            ? "Too few located stops to optimise; original order kept."
            : $"Re-sequenced {located.Count} located stop(s): {originalDistance:0.0} km -> {proposedDistance:0.0} km "
              + $"({(saving >= 0 ? "-" : "+")}{Math.Abs(saving):0.0} km), est. {Math.Round(totalMinutes)} min.";

        return new AiRouteOptimizationResult(
            Name,
            summary,
            Math.Round(originalDistance, 2),
            Math.Round(proposedDistance, 2),
            (int)Math.Round(totalMinutes),
            confidence,
            ordered);
    }

    private static List<AiRouteStopInput> NearestNeighbour(
        double? startLat, double? startLon, List<AiRouteStopInput> stops)
    {
        var pool = new List<AiRouteStopInput>(stops);
        var result = new List<AiRouteStopInput>(stops.Count);
        double? lat = startLat;
        double? lon = startLon;

        if (lat is null || lon is null)
        {
            if (pool.Count == 0)
            {
                return result;
            }

            var seed = pool[0];
            result.Add(seed);
            pool.RemoveAt(0);
            lat = seed.Latitude;
            lon = seed.Longitude;
        }

        while (pool.Count > 0)
        {
            var cLat = lat!.Value;
            var cLon = lon!.Value;
            var next = pool
                .OrderBy(s => Haversine(cLat, cLon, s.Latitude!.Value, s.Longitude!.Value))
                .ThenBy(s => s.OriginalSequence)
                .First();
            result.Add(next);
            pool.Remove(next);
            lat = next.Latitude;
            lon = next.Longitude;
        }

        return result;
    }

    private static void TwoOpt(double? startLat, double? startLon, List<AiRouteStopInput> tour)
    {
        if (tour.Count < 4)
        {
            return;
        }

        var improved = true;
        var passes = 0;
        while (improved && passes++ < MaxTwoOptPasses)
        {
            improved = false;
            for (var i = 0; i < tour.Count - 1; i++)
            {
                for (var k = i + 1; k < tour.Count; k++)
                {
                    var before = SegmentCost(startLat, startLon, tour, i, k);
                    tour.Reverse(i, k - i + 1);
                    var after = SegmentCost(startLat, startLon, tour, i, k);
                    if (after + 1e-6 < before)
                    {
                        improved = true;
                    }
                    else
                    {
                        tour.Reverse(i, k - i + 1);
                    }
                }
            }
        }
    }

    private static double SegmentCost(
        double? startLat, double? startLon, List<AiRouteStopInput> tour, int i, int k)
    {
        double? prevLat = i == 0 ? startLat : tour[i - 1].Latitude;
        double? prevLon = i == 0 ? startLon : tour[i - 1].Longitude;
        double cost = 0;
        for (var idx = i; idx <= k && idx < tour.Count; idx++)
        {
            if (prevLat.HasValue && prevLon.HasValue)
            {
                cost += Haversine(prevLat.Value, prevLon.Value, tour[idx].Latitude!.Value, tour[idx].Longitude!.Value);
            }

            prevLat = tour[idx].Latitude;
            prevLon = tour[idx].Longitude;
        }

        if (k + 1 < tour.Count && prevLat.HasValue && prevLon.HasValue)
        {
            cost += Haversine(prevLat.Value, prevLon.Value, tour[k + 1].Latitude!.Value, tour[k + 1].Longitude!.Value);
        }

        return cost;
    }

    private static double PathDistance(double? startLat, double? startLon, List<AiRouteStopInput> order)
    {
        double total = 0;
        double? prevLat = startLat;
        double? prevLon = startLon;
        foreach (var stop in order)
        {
            if (prevLat.HasValue && prevLon.HasValue && stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                total += Haversine(prevLat.Value, prevLon.Value, stop.Latitude.Value, stop.Longitude.Value);
            }

            if (stop.Latitude.HasValue && stop.Longitude.HasValue)
            {
                prevLat = stop.Latitude;
                prevLon = stop.Longitude;
            }
        }

        return total;
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
