using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Data.Seed;

// Public tourism content (heritage places, villages, cuisines, festivals, events, routes and a few clearly
// marked DEMO tourist services), loaded from Seed/HeritageData/heritage-discovery.json so the database is the
// single source of truth for the Tourism pages. Everything is insert-if-missing (matched by name): existing rows
// are never modified, except that empty descriptive columns on District are filled in. Admin edits therefore
// survive restarts -- but a seeded row an admin DELETES will be re-created, so deactivate instead of deleting.
public static class HeritageDiscoverySeeder
{
    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Seed", "HeritageData", "heritage-discovery.json");
        if (!File.Exists(path))
        {
            return;
        }

        var data = JsonSerializer.Deserialize<SeedFile>(
            await File.ReadAllTextAsync(path, cancellationToken), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (data is null)
        {
            return;
        }

        var districts = await context.Districts.ToListAsync(cancellationToken);
        var districtByName = districts.ToDictionary(d => Normalise(d.Name), d => d);
        Guid? DistrictId(string name) => districtByName.TryGetValue(Normalise(name), out var d) ? d.Id : null;

        foreach (var item in data.Districts)
        {
            if (!districtByName.TryGetValue(Normalise(item.Name), out var district))
            {
                continue;
            }

            district.Description ??= item.Description;
            district.KnownFor ??= item.KnownFor;
            district.SourceUrl ??= item.SourceUrl;
            district.ImageUrl ??= item.ImageUrl;
            district.ImageCredit ??= item.ImageCredit;
        }

        var now = DateTime.UtcNow;

        var placeNames = await context.HeritagePlaces.Select(p => p.Name).ToListAsync(cancellationToken);
        var knownPlaces = new HashSet<string>(placeNames, StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Places)
        {
            var districtId = DistrictId(item.District);
            if (districtId is null || knownPlaces.Contains(item.Name) || !Enum.TryParse<HeritagePlaceType>(item.PlaceType, out var type))
            {
                continue;
            }

            context.HeritagePlaces.Add(new HeritagePlace
            {
                Id = Guid.NewGuid(), Name = item.Name, Description = item.Description, PlaceType = type, DistrictId = districtId.Value,
                Latitude = item.Latitude, Longitude = item.Longitude, KnownFor = item.KnownFor, SourceUrl = item.SourceUrl,
                ImageUrl = item.ImageUrl, ImageCredit = item.ImageCredit, IsActive = true, CreatedAt = now, UpdatedAt = now,
            });
        }
        await context.SaveChangesAsync(cancellationToken);

        var villageNames = new HashSet<string>(await context.Villages.Select(v => v.Name).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Villages)
        {
            var districtId = DistrictId(item.District);
            if (districtId is null || villageNames.Contains(item.Name))
            {
                continue;
            }

            context.Villages.Add(new Village
            {
                Id = Guid.NewGuid(), Name = item.Name, Craft = item.Craft, Description = item.Description, DistrictId = districtId.Value,
                VisitTips = item.VisitTips, SourceUrl = item.SourceUrl, SourceLabel = item.SourceLabel,
                ImageUrl = item.ImageUrl, ImageCredit = item.ImageCredit, IsActive = true, CreatedAt = now, UpdatedAt = now,
            });
        }

        var cuisineNames = new HashSet<string>(await context.LocalCuisines.Select(c => c.Name).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Cuisines)
        {
            var districtId = DistrictId(item.District);
            if (districtId is null || cuisineNames.Contains(item.Name))
            {
                continue;
            }

            context.LocalCuisines.Add(new LocalCuisine
            {
                Id = Guid.NewGuid(), Name = item.Name, Description = item.Description, WhereToTry = item.WhereToTry, DistrictId = districtId.Value,
                Kind = item.Kind, Ingredients = item.Ingredients, SourceUrl = item.SourceUrl, IsNationwide = item.Nationwide,
                IsActive = true, CreatedAt = now, UpdatedAt = now,
            });
        }

        var festivalNames = new HashSet<string>(await context.HeritageFestivals.Select(f => f.Name).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Festivals)
        {
            var districtId = DistrictId(item.District);
            if (districtId is null || festivalNames.Contains(item.Name))
            {
                continue;
            }

            var (start, end) = NextOccurrence(item.Month, item.Day, item.Days, now);
            context.HeritageFestivals.Add(new HeritageFestival
            {
                Id = Guid.NewGuid(), Name = item.Name, Description = item.Description, DistrictId = districtId.Value,
                StartDate = start, EndDate = end, IsRecurringAnnually = true, IsActive = true, CreatedAt = now, UpdatedAt = now,
            });
        }

        var eventNames = new HashSet<string>(await context.CulturalEvents.Select(e => e.Name).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Events)
        {
            var districtId = DistrictId(item.District);
            if (districtId is null || eventNames.Contains(item.Name))
            {
                continue;
            }

            var (start, end) = NextOccurrence(item.Month, item.Day, item.Days, now);
            context.CulturalEvents.Add(new CulturalEvent
            {
                Id = Guid.NewGuid(), Name = item.Name, Description = item.Description, Category = item.Category, DistrictId = districtId.Value,
                EventDate = start, EndDate = end, IsActive = true, CreatedAt = now, UpdatedAt = now,
            });
        }
        await context.SaveChangesAsync(cancellationToken);

        await SeedRoutesAsync(context, data, now, cancellationToken);
        await SeedDemoServicesAsync(context, data, districtByName.ToDictionary(k => k.Key, v => v.Value.Id), now, cancellationToken);
    }

    private static async Task SeedRoutesAsync(ShilpoHubDbContext context, SeedFile data, DateTime now, CancellationToken cancellationToken)
    {
        var routeNames = new HashSet<string>(await context.HeritageRoutes.Select(r => r.Name).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        var placeByName = (await context.HeritagePlaces.ToListAsync(cancellationToken))
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var nameByKey = data.Places.ToDictionary(p => p.Key, p => p.Name);

        foreach (var item in data.Routes)
        {
            var stops = item.Stops.Select(k => nameByKey.TryGetValue(k, out var n) && placeByName.TryGetValue(n, out var p) ? p : null).ToList();
            if (routeNames.Contains(item.Name) || stops.Any(s => s is null) || stops.Count < 2)
            {
                continue;
            }

            Enum.TryParse<TransportationMode>(item.Mode, out var mode);
            var route = new HeritageRoute
            {
                Id = Guid.NewGuid(), Name = item.Name, Description = item.Description, Status = RouteStatus.Published,
                IsRecommended = true, CreatedAt = now, UpdatedAt = now,
            };

            double total = 0;
            var order = 1;
            HeritagePlace? previous = null;
            foreach (var place in stops)
            {
                // Straight-line distance x 1.4 road factor, at ~40 km/h -- clearly an estimate (see route description).
                var km = previous is null ? (double?)null : Math.Round(Haversine(previous, place!) * 1.4, 1);
                total += km ?? 0;
                route.Stops.Add(new RouteStop
                {
                    Id = Guid.NewGuid(), RouteId = route.Id, HeritagePlaceId = place!.Id, Order = order++,
                    DistanceFromPreviousKm = km, TransportationMode = mode,
                    EstimatedTravelMinutesFromPrevious = km is null ? null : (int)Math.Round(km.Value / 40 * 60),
                });
                previous = place;
            }

            route.TotalDistanceKm = Math.Round(total, 1);
            route.EstimatedDurationMinutes = (int)Math.Round(total / 40 * 60) + stops.Count * 60;
            context.HeritageRoutes.Add(route);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoServicesAsync(
        ShilpoHubDbContext context, SeedFile data, Dictionary<string, Guid> districtIds, DateTime now, CancellationToken cancellationToken)
    {
        // Demo services need an owner; without a Producer account there is nothing to attach them to.
        var producerId = await context.UserRoles.Where(ur => ur.Role.Name == RoleNames.Producer)
            .OrderBy(ur => ur.AssignedAt).Select(ur => (Guid?)ur.UserId).FirstOrDefaultAsync(cancellationToken);
        if (producerId is null)
        {
            return;
        }

        var titles = new HashSet<string>(await context.TouristServices.Select(s => s.Title).ToListAsync(cancellationToken), StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Services)
        {
            if (titles.Contains(item.Title) || !districtIds.TryGetValue(Normalise(item.District), out var districtId)
                || !Enum.TryParse<BookingType>(item.Type, out var type))
            {
                continue;
            }

            var service = new TouristService
            {
                Id = Guid.NewGuid(), Title = item.Title, Description = item.Description, Type = type, Price = item.Price,
                DurationMinutes = item.Duration, DefaultCapacity = item.Capacity, Location = item.Location,
                ProducerId = producerId.Value, DistrictId = districtId, IsActive = true, CreatedAt = now, UpdatedAt = now,
            };

            // Six weekly slots, 10:00 Bangladesh time, so the booking flow can be tried straight away.
            for (var week = 1; week <= 6; week++)
            {
                var start = now.Date.AddDays(week * 7).AddHours(4);
                service.AvailabilitySlots.Add(new ServiceAvailabilitySlot
                {
                    Id = Guid.NewGuid(), ServiceId = service.Id, StartAt = start,
                    EndAt = start.AddMinutes(item.Duration ?? 24 * 60), Capacity = item.Capacity, IsActive = true, CreatedAt = now, UpdatedAt = now,
                });
            }

            context.TouristServices.Add(service);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // Next occurrence of a yearly date (this year if it has not yet ended, otherwise next year), UTC.
    private static (DateTime Start, DateTime End) NextOccurrence(int month, int day, int days, DateTime now)
    {
        for (var year = now.Year; ; year++)
        {
            var start = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddDays(Math.Max(days, 1)).AddSeconds(-1);
            if (end >= now)
            {
                return (start, end);
            }
        }
    }

    private static double Haversine(HeritagePlace a, HeritagePlace b)
    {
        const double R = 6371;
        double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(b.Latitude - a.Latitude);
        var dLon = Rad(b.Longitude - a.Longitude);
        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(Rad(a.Latitude)) * Math.Cos(Rad(b.Latitude)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    // Spelling variants across sources ("Cox’s Bazar"/"Cox's Bazar", "Barisal"/"Barishal", ...).
    private static string Normalise(string value) => value.ToLowerInvariant().Replace("’", "").Replace("'", "").Trim()
        .Replace("barisal", "barishal").Replace("chittagong", "chattogram").Replace("comilla", "cumilla").Replace("jessore", "jashore")
        .Replace("bogra", "bogura").Replace("maulvibazar", "moulvibazar").Replace("chapai nawabganj", "chapainawabganj");

    private sealed class SeedFile
    {
        public List<DistrictItem> Districts { get; set; } = new();
        public List<PlaceItem> Places { get; set; } = new();
        public List<VillageItem> Villages { get; set; } = new();
        public List<CuisineItem> Cuisines { get; set; } = new();
        public List<DatedItem> Festivals { get; set; } = new();
        public List<DatedItem> Events { get; set; } = new();
        public List<RouteItem> Routes { get; set; } = new();
        public List<ServiceItem> Services { get; set; } = new();
    }

    private sealed class DistrictItem { public string Name { get; set; } = ""; public string? Description { get; set; } public string? KnownFor { get; set; } public string? SourceUrl { get; set; } public string? ImageUrl { get; set; } public string? ImageCredit { get; set; } }
    private sealed class PlaceItem { public string Key { get; set; } = ""; public string Name { get; set; } = ""; public string District { get; set; } = ""; public string PlaceType { get; set; } = ""; public string Description { get; set; } = ""; public string? KnownFor { get; set; } public double Latitude { get; set; } public double Longitude { get; set; } public string? SourceUrl { get; set; } public string? ImageUrl { get; set; } public string? ImageCredit { get; set; } }
    private sealed class VillageItem { public string Name { get; set; } = ""; public string District { get; set; } = ""; public string Craft { get; set; } = ""; public string? Description { get; set; } public string? VisitTips { get; set; } public string? SourceUrl { get; set; } public string? SourceLabel { get; set; } public string? ImageUrl { get; set; } public string? ImageCredit { get; set; } }
    private sealed class CuisineItem { public string Name { get; set; } = ""; public string District { get; set; } = ""; public bool Nationwide { get; set; } public string? Kind { get; set; } public string Description { get; set; } = ""; public string? WhereToTry { get; set; } public string? Ingredients { get; set; } public string? SourceUrl { get; set; } }
    private sealed class DatedItem { public string Name { get; set; } = ""; public string District { get; set; } = ""; public string Category { get; set; } = ""; public int Month { get; set; } public int Day { get; set; } public int Days { get; set; } = 1; public string Description { get; set; } = ""; }
    private sealed class RouteItem { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Mode { get; set; } = "Car"; public List<string> Stops { get; set; } = new(); }
    private sealed class ServiceItem { public string Title { get; set; } = ""; public string Type { get; set; } = ""; public string District { get; set; } = ""; public string? Location { get; set; } public decimal Price { get; set; } public int? Duration { get; set; } public int Capacity { get; set; } = 1; public string Description { get; set; } = ""; }
}
