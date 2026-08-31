using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.AITourism;

// Rule-based stand-in for a future AI/ML backend (Gemini, OpenAI, custom model, etc). Every method
// derives its answer from simple heuristics over the context it is given -- no external calls, no
// model weights. Swap this out for a real implementation of IAITourismProvider later; nothing in
// AITourismService or the controller needs to change.
public class DummyAITourismProvider : IAITourismProvider
{
    private const double EarthRadiusKm = 6371.0;
    private const int PlacesPerDay = 3;

    public Task<TourPlanResult> PlanTourAsync(TourPlanContext context, CancellationToken cancellationToken)
    {
        var placeStops = context.Places
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.Name)
            .Select(p => new TourStopDto { ReferenceId = p.Id, Type = "HeritagePlace", Name = p.Name, Notes = p.PlaceType })
            .ToList();

        var serviceStops = context.Services
            .OrderBy(s => s.Price)
            .Select(s => new TourStopDto
            {
                ReferenceId = s.Id,
                Type = "TouristService",
                Name = s.Title,
                Notes = $"{s.Type} - starts from ৳{s.Price:N0}",
            })
            .ToList();

        var days = new List<TourDayPlanDto>();
        var placeIndex = 0;
        var serviceIndex = 0;

        for (var day = 1; day <= Math.Max(1, context.DurationDays); day++)
        {
            var stops = new List<TourStopDto>();

            for (var i = 0; i < PlacesPerDay && placeIndex < placeStops.Count; i++)
            {
                stops.Add(placeStops[placeIndex++]);
            }

            if (day % 2 == 0 && serviceIndex < serviceStops.Count)
            {
                stops.Add(serviceStops[serviceIndex++]);
            }

            if (stops.Count == 0)
            {
                stops.Add(new TourStopDto
                {
                    Type = "FreeTime",
                    Name = "Free time / local exploration",
                    Notes = "No more curated stops are available for this district yet.",
                });
            }

            days.Add(new TourDayPlanDto
            {
                DayNumber = day,
                Date = context.StartDate?.AddDays(day - 1),
                Stops = stops,
            });
        }

        var highlightedFestivals = context.Festivals
            .Where(f => !context.StartDate.HasValue || TripOverlaps(f, context.StartDate.Value, context.DurationDays))
            .OrderBy(f => f.StartDate)
            .Take(3)
            .Select(f => $"{f.Name} ({f.StartDate:MMM d} - {f.EndDate:MMM d})")
            .ToList();

        var coveredPlaces = Math.Min(placeStops.Count, placeIndex);
        var coveredServices = Math.Min(serviceStops.Count, serviceIndex);
        var summary =
            $"{context.DurationDays}-day itinerary for {context.PartySize} traveler(s) in {context.DistrictName}, " +
            $"covering {coveredPlaces} heritage site(s)" +
            (coveredServices > 0 ? $" and {coveredServices} curated experience(s)." : ".");

        return Task.FromResult(new TourPlanResult
        {
            Days = days,
            HighlightedFestivals = highlightedFestivals,
            Summary = summary,
        });
    }

    public Task<BudgetPlanResult> PlanBudgetAsync(BudgetPlanContext context, CancellationToken cancellationToken)
    {
        var lineItems = context.ServiceLines
            .Select(line => new BudgetLineItemDto
            {
                Label = line.Title,
                Category = line.Type,
                Amount = line.UnitPrice * line.PartySize,
            })
            .ToList();

        var foodTotal = context.DailyFoodBudgetPerPerson * context.PartySize * context.DurationDays;
        if (foodTotal > 0)
        {
            lineItems.Add(new BudgetLineItemDto { Label = "Meals", Category = "Food", Amount = foodTotal });
        }

        var miscTotal = context.DailyMiscBudgetPerPerson * context.PartySize * context.DurationDays;
        if (miscTotal > 0)
        {
            lineItems.Add(new BudgetLineItemDto { Label = "Miscellaneous & souvenirs", Category = "Miscellaneous", Amount = miscTotal });
        }

        var total = lineItems.Sum(l => l.Amount);
        var perPerson = context.PartySize > 0 ? total / context.PartySize : total;

        var notes = context.ServiceLines.Count == 0
            ? "No services selected yet -- add guide, workshop, homestay or transport bookings to refine this estimate."
            : $"Estimate covers {context.ServiceLines.Count} selected service(s) plus {context.DurationDays} day(s) of meals and incidentals.";

        return Task.FromResult(new BudgetPlanResult
        {
            LineItems = lineItems,
            TotalEstimatedCost = Math.Round(total, 2),
            PerPersonCost = Math.Round(perPerson, 2),
            Notes = notes,
        });
    }

    public Task<RouteOptimizationResult> OptimizeRouteAsync(RouteOptimizationContext context, CancellationToken cancellationToken)
    {
        var remaining = new List<RoutePlaceDto>(context.Places);
        var stops = new List<OptimizedStopDto>();

        if (remaining.Count == 0)
        {
            return Task.FromResult(new RouteOptimizationResult
            {
                Stops = stops,
                TotalDistanceKm = 0,
                Notes = "No places were provided to build a route from.",
            });
        }

        double currentLat;
        double currentLng;

        if (context.StartLatitude.HasValue && context.StartLongitude.HasValue)
        {
            currentLat = context.StartLatitude.Value;
            currentLng = context.StartLongitude.Value;
        }
        else
        {
            currentLat = remaining[0].Latitude;
            currentLng = remaining[0].Longitude;
        }

        var totalDistance = 0.0;
        var order = 1;

        while (remaining.Count > 0)
        {
            var nearest = remaining
                .Select(p => (Place: p, Distance: HaversineDistanceKm(currentLat, currentLng, p.Latitude, p.Longitude)))
                .OrderBy(x => x.Distance)
                .First();

            stops.Add(new OptimizedStopDto
            {
                PlaceId = nearest.Place.Id,
                Name = nearest.Place.Name,
                Order = order++,
                DistanceFromPreviousKm = Math.Round(nearest.Distance, 2),
            });

            totalDistance += nearest.Distance;
            currentLat = nearest.Place.Latitude;
            currentLng = nearest.Place.Longitude;
            remaining.Remove(nearest.Place);
        }

        return Task.FromResult(new RouteOptimizationResult
        {
            Stops = stops,
            TotalDistanceKm = Math.Round(totalDistance, 2),
            Notes = $"Nearest-neighbor route covering {stops.Count} stop(s) with an estimated {Math.Round(totalDistance, 1)} km of travel.",
        });
    }

    public Task<TourismTranslationResult> TranslateAsync(TourismTranslationRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TourismTranslationResult
        {
            OriginalText = request.Text,
            TranslatedText = $"[{request.TargetLanguage}] {request.Text}",
            TargetLanguage = request.TargetLanguage,
        });
    }

    public Task<CulturalRecommendationResult> RecommendAsync(CulturalRecommendationContext context, CancellationToken cancellationToken)
    {
        var interests = context.Interests
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToList();

        var items = new List<RecommendationItemDto>();

        foreach (var place in context.Places)
        {
            var (score, reason) = ScorePlace(place, interests);
            items.Add(new RecommendationItemDto
            {
                Id = place.Id,
                Type = "HeritagePlace",
                Name = place.Name,
                Description = place.Description,
                Score = score,
                Reason = reason,
            });
        }

        foreach (var festival in context.Festivals)
        {
            var (score, reason) = ScoreDated(festival.Name, festival.Description, festival.StartDate, interests);
            items.Add(new RecommendationItemDto
            {
                Id = festival.Id,
                Type = "Festival",
                Name = festival.Name,
                Description = festival.Description,
                Score = score,
                Reason = reason,
            });
        }

        foreach (var culturalEvent in context.Events)
        {
            var (score, reason) = ScoreDated(culturalEvent.Name, culturalEvent.Description, culturalEvent.EventDate, interests);
            items.Add(new RecommendationItemDto
            {
                Id = culturalEvent.Id,
                Type = "CulturalEvent",
                Name = culturalEvent.Name,
                Description = culturalEvent.Description,
                Score = score,
                Reason = reason,
            });
        }

        foreach (var cuisine in context.Cuisines)
        {
            var matches = MatchesInterests(cuisine.Name, cuisine.Description, interests);
            items.Add(new RecommendationItemDto
            {
                Id = cuisine.Id,
                Type = "LocalCuisine",
                Name = cuisine.Name,
                Description = cuisine.Description,
                Score = matches ? 80m : 50m,
                Reason = matches ? "Matches your interests." : "Popular local dish worth trying.",
            });
        }

        var top = items
            .OrderByDescending(i => i.Score)
            .ThenBy(i => i.Name)
            .Take(Math.Max(1, context.MaxResults))
            .ToList();

        return Task.FromResult(new CulturalRecommendationResult { Recommendations = top });
    }

    private static (decimal Score, string Reason) ScorePlace(HeritagePlaceSummaryDto place, List<string> interests)
    {
        decimal score = 50m;
        var reasons = new List<string>();

        if (place.IsFeatured)
        {
            score += 25m;
            reasons.Add("featured heritage site");
        }

        if (MatchesInterests(place.Name, place.Description, interests)
            || interests.Any(i => place.PlaceType.Contains(i, StringComparison.OrdinalIgnoreCase)))
        {
            score += 25m;
            reasons.Add("matches your interests");
        }

        var reason = reasons.Count > 0 ? $"Recommended: {string.Join(", ", reasons)}." : "Popular heritage destination.";
        return (Math.Min(100m, score), reason);
    }

    private static (decimal Score, string Reason) ScoreDated(string name, string description, DateTime date, List<string> interests)
    {
        decimal score = 40m;
        var reasons = new List<string>();

        var daysUntil = (date.Date - DateTime.UtcNow.Date).Days;
        if (daysUntil is >= 0 and <= 30)
        {
            score += 30m;
            reasons.Add("happening soon");
        }
        else if (daysUntil < 0)
        {
            score -= 20m;
        }

        if (MatchesInterests(name, description, interests))
        {
            score += 30m;
            reasons.Add("matches your interests");
        }

        var reason = reasons.Count > 0 ? $"Recommended: {string.Join(", ", reasons)}." : "Cultural highlight in this district.";
        return (Math.Clamp(score, 0m, 100m), reason);
    }

    private static bool MatchesInterests(string name, string description, List<string> interests)
        => interests.Count > 0 && interests.Any(i =>
            name.Contains(i, StringComparison.OrdinalIgnoreCase) || description.Contains(i, StringComparison.OrdinalIgnoreCase));

    private static bool TripOverlaps(HeritageFestivalSummaryDto festival, DateTime tripStart, int durationDays)
    {
        var tripEnd = tripStart.AddDays(Math.Max(1, durationDays) - 1);
        return festival.StartDate.Date <= tripEnd.Date && festival.EndDate.Date >= tripStart.Date;
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
}
