using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Seed;

// Sample/demo tourism locations for testing the Admin-managed map + AI Planner. Every row is
// inserted with IsVerified = false -- per this project's convention, unverified locations are the
// signal that a record is sample data, not a confirmed live listing. Real districts are resolved
// by name at seed time rather than hardcoding district ids.
public static class TourismLocationSeeder
{
    // Cox's Bazar sample rows that earlier versions of this seeder inserted (with placeholder phone
    // numbers). They are replaced by the sourced records in Seed/TourismData/coxsbazar.json and are
    // deactivated -- not deleted -- so nothing that referenced them is orphaned.
    private static readonly string[] RetiredSampleNames =
    {
        "Sea Pearl Beachfront Hotel",
        "Himchari Palm Resort",
        "Marine Drive Backpackers Hostel",
        "Laboni Beach Point",
        "Himchari National Park",
        "Inani Beach Seafood House",
    };

    private static readonly (string District, string Name, TourismLocationType Type, string Description, double Lat, double Lng,
        decimal? Price, decimal? EntryFee, string? Hours, string? Contact, string? Facilities)[] Locations =
    {
        ("Dhaka", "Old Town Heritage Hotel", TourismLocationType.Hotel,
            "Sample listing -- a mid-range hotel near Old Dhaka's heritage sites. Verify details before booking.",
            23.7104, 90.4074, 3800m, null, null, "+880-1XXX-000005", "AC, Wi-Fi, breakfast included"),
        ("Dhaka", "Lalbagh Fort", TourismLocationType.HeritageSite,
            "Sample listing -- a 17th-century Mughal fort complex in Old Dhaka.",
            23.7189, 90.3881, null, 20m, "9:00 AM - 5:00 PM (closed Sundays)", null, "Garden, museum"),
        ("Dhaka", "Dhaka Family Restaurant", TourismLocationType.Restaurant,
            "Sample listing -- a family restaurant serving traditional Bengali cuisine. Verify details before booking.",
            23.7461, 90.3742, null, null, "12:00 PM - 11:00 PM", "+880-1XXX-000006", "Bengali cuisine, AC seating"),
        ("Dhaka", "Riverside Backpackers Hostel", TourismLocationType.Hostel,
            "Sample listing -- a budget hostel near Sadarghat river terminal. Verify details before booking.",
            23.7060, 90.4070, 700m, null, null, "+880-1XXX-000007", "Dorm beds, common room"),

        ("Sylhet", "Tea Garden View Resort", TourismLocationType.Resort,
            "Sample listing -- a resort overlooking the tea estates outside Sylhet town. Verify details before booking.",
            24.8500, 91.8000, 5200m, null, null, "+880-1XXX-000008", "Tea garden view, restaurant, parking"),
        ("Sylhet", "Ratargul Swamp Forest", TourismLocationType.TouristPlace,
            "Sample listing -- a freshwater swamp forest accessible by boat, popular for eco-tourism.",
            25.0167, 91.9333, null, 100m, "9:00 AM - 5:00 PM", null, "Boat rides"),
        ("Sylhet", "Shrine Side Hotel", TourismLocationType.Hotel,
            "Sample listing -- a hotel near the Hazrat Shah Jalal shrine in Sylhet town. Verify details before booking.",
            24.8988, 91.8697, 3200m, null, null, "+880-1XXX-000009", "AC, Wi-Fi"),
        ("Sylhet", "Sylhet Highland Restaurant", TourismLocationType.Restaurant,
            "Sample listing -- a restaurant known for regional Sylheti dishes. Verify details before booking.",
            24.8949, 91.8687, null, null, "11:00 AM - 10:00 PM", "+880-1XXX-000010", "Regional cuisine"),

        ("Bandarban", "Nilgiri Hill Resort", TourismLocationType.Resort,
            "Sample listing -- a hillside resort with panoramic views near Nilgiri. Verify details before booking.",
            21.9333, 92.3167, 5800m, null, null, "+880-1XXX-000011", "Hill view, restaurant, generator backup"),
        ("Bandarban", "Nilachol Viewpoint", TourismLocationType.Attraction,
            "Sample listing -- a popular hilltop viewpoint overlooking the Bandarban valley.",
            22.1900, 92.2100, null, 30m, "8:00 AM - 6:00 PM", null, "Viewpoint, small cafe"),
        ("Bandarban", "Bandarban Town Hostel", TourismLocationType.Hostel,
            "Sample listing -- a budget hostel in Bandarban town, popular with trekkers. Verify details before booking.",
            22.1953, 92.2184, 600m, null, null, "+880-1XXX-000012", "Dorm beds, luggage storage"),
        ("Bandarban", "Golden Temple (Buddha Dhatu Jadi)", TourismLocationType.HeritageSite,
            "Sample listing -- the largest Buddhist temple in Bangladesh, on a hill near Bandarban town.",
            22.1975, 92.2226, null, 20m, "8:00 AM - 6:00 PM", null, "Temple grounds, viewpoint"),
    };

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var districts = await context.Districts.ToListAsync(cancellationToken);
        var districtByName = districts.ToDictionary(d => d.Name, d => d.Id, StringComparer.OrdinalIgnoreCase);

        // Imported OpenStreetMap rows are not seed data (and may legitimately share a name within a district).
        var existing = await context.TourismLocations.Where(l => l.Source != "OpenStreetMap").ToListAsync(cancellationToken);
        var existingByKey = existing.GroupBy(l => (l.DistrictId, l.Name)).ToDictionary(g => g.Key, g => g.First());
        var now = DateTime.UtcNow;

        foreach (var item in Locations)
        {
            if (!districtByName.TryGetValue(item.District, out var districtId))
            {
                continue;
            }

            if (existingByKey.ContainsKey((districtId, item.Name)))
            {
                continue;
            }

            context.TourismLocations.Add(new TourismLocation
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Type = item.Type,
                Description = item.Description,
                DistrictId = districtId,
                Latitude = item.Lat,
                Longitude = item.Lng,
                Price = item.Price,
                EntryFee = item.EntryFee,
                OpeningHours = item.Hours,
                ContactInfo = item.Contact,
                Facilities = item.Facilities,
                IsVerified = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        await RetireSampleRowsAsync(context, cancellationToken);
        await ImportSourcedDataAsync(context, cancellationToken);
    }

    private static async Task RetireSampleRowsAsync(ShilpoHubDbContext context, CancellationToken cancellationToken)
    {
        var stale = await context.TourismLocations
            .Where(l => RetiredSampleNames.Contains(l.Name) && l.SourceUrl == null && !l.IsVerified && l.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var location in stale)
        {
            location.IsActive = false;
        }

        if (stale.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    // Imports every Seed/TourismData/<district>.json (copied next to the binaries). One file per
    // district, same format -- adding Sylhet, Bandarban, ... is just another file. A location with
    // no coordinates is skipped (the map needs a point); nothing is ever guessed to fill a gap.
    private static async Task ImportSourcedDataAsync(ShilpoHubDbContext context, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Seed", "TourismData");
        if (!Directory.Exists(folder))
        {
            return;
        }

        var districtByName = (await context.Districts.ToListAsync(cancellationToken))
            .ToDictionary(d => d.Name, d => d.Id, StringComparer.OrdinalIgnoreCase);
        var existing = (await context.TourismLocations.Where(l => l.Source != "OpenStreetMap").ToListAsync(cancellationToken))
            .GroupBy(l => (l.DistrictId, l.Name)).ToDictionary(g => g.Key, g => g.First());
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var now = DateTime.UtcNow;

        foreach (var file in Directory.EnumerateFiles(folder, "*.json").OrderBy(f => f))
        {
            var data = JsonSerializer.Deserialize<TourismDataFile>(await File.ReadAllTextAsync(file, cancellationToken), options);
            if (data is null || !districtByName.TryGetValue(data.District, out var districtId))
            {
                continue;
            }

            var retrievedOn = DateTime.SpecifyKind(DateTime.Parse(data.RetrievedOn), DateTimeKind.Utc);

            foreach (var item in data.Locations)
            {
                if (item.Latitude is null || item.Longitude is null
                    || !Enum.TryParse<TourismLocationType>(item.Type, ignoreCase: true, out var type))
                {
                    continue;
                }

                var isNew = !existing.TryGetValue((districtId, item.Name), out var location);
                if (isNew)
                {
                    location = new TourismLocation { Id = Guid.NewGuid(), DistrictId = districtId, CreatedAt = now, UpdatedAt = now };
                    context.TourismLocations.Add(location);
                }
                else
                {
                    // A retired sample row with the same name is taken over by the sourced record;
                    // otherwise never overwrite an admin-created / hand-edited / already-current row.
                    var isRetiredSample = location!.SourceUrl is null && RetiredSampleNames.Contains(location.Name);
                    var isStaleImport = location.SourceUrl is not null && location.UpdatedAt == location.CreatedAt
                        && location.DataRetrievedOn < retrievedOn;
                    if (!isRetiredSample && !isStaleImport)
                    {
                        // A photo is presentation, not a fact: an imported row with no photo (or one
                        // from an earlier import) picks up the file's photo without a full refresh.
                        var mayTakePhoto = location.SourceUrl is not null && (location.ImageUrl is null || location.ImageCredit is not null);
                        if (mayTakePhoto && item.ImageUrl is not null && location.ImageUrl != item.ImageUrl)
                        {
                            location.ImageUrl = item.ImageUrl;
                            location.ImageCredit = item.ImageCredit;
                            location.ImageSourceUrl = item.ImageSourceUrl;
                        }

                        continue;
                    }
                }

                location!.Name = item.Name;
                location.Type = type;
                location.Description = item.Description;
                location.Address = item.Address;
                location.Area = item.Area;
                location.Latitude = item.Latitude.Value;
                location.Longitude = item.Longitude.Value;
                location.Price = item.Price;
                location.EntryFee = item.EntryFee;
                location.OpeningHours = item.OpeningHours;
                location.ContactInfo = item.Contact;
                location.Facilities = item.Facilities;
                if (item.ImageUrl is not null)
                {
                    location.ImageUrl = item.ImageUrl;
                    location.ImageCredit = item.ImageCredit;
                    location.ImageSourceUrl = item.ImageSourceUrl;
                }

                location.SourceUrl = item.SourceUrl;
                location.VerificationStatus = item.VerificationStatus;
                location.IsVerified = item.VerificationStatus == "Verified";
                location.UnverifiedFields = item.UnverifiedFields is { Count: > 0 } ? string.Join("; ", item.UnverifiedFields) : null;
                location.CoordinatesSource = item.CoordinatesSource;
                location.CoordinatesPrecision = item.CoordinatesPrecision;
                location.DataRetrievedOn = retrievedOn;
                location.IsActive = true;
                if (!isNew)
                {
                    location.UpdatedAt = location.CreatedAt;   // keep "unedited" so a later refresh still applies
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed class TourismDataFile
    {
        public string District { get; set; } = string.Empty;
        public string RetrievedOn { get; set; } = string.Empty;
        public List<TourismDataItem> Locations { get; set; } = new();
    }

    private sealed class TourismDataItem
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Area { get; set; }
        public string? Address { get; set; }
        public string Description { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoordinatesSource { get; set; }
        public string? CoordinatesPrecision { get; set; }
        public string? Contact { get; set; }
        public decimal? Price { get; set; }
        public decimal? EntryFee { get; set; }
        public string? OpeningHours { get; set; }
        public string? Facilities { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageCredit { get; set; }
        public string? ImageSourceUrl { get; set; }
        public string? SourceUrl { get; set; }
        public string VerificationStatus { get; set; } = "Unverified";
        public List<string>? UnverifiedFields { get; set; }
    }
}
