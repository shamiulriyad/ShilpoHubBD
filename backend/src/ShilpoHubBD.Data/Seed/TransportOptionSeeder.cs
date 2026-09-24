using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Seed;

// Imports every Seed/TransportData/<route>.json (copied next to the binaries): which buses, trains
// and airlines serve an origin -> destination-district route, with only the facts a source states.
// One file per route -- adding Dhaka -> Sylhet is just another file. Same insert / refresh rules as
// TourismLocationSeeder: rows are matched by (origin, destination, mode, service name) and updated
// only when the file is newer.
public static class TransportOptionSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Seed", "TransportData");
        if (!Directory.Exists(folder))
        {
            return;
        }

        var existing = (await context.TransportOptions.ToListAsync(cancellationToken))
            .ToDictionary(t => (t.OriginName, t.DestinationDistrict, t.Mode, t.ServiceName), t => t);

        foreach (var file in Directory.EnumerateFiles(folder, "*.json").OrderBy(f => f))
        {
            var data = JsonSerializer.Deserialize<TransportDataFile>(await File.ReadAllTextAsync(file, cancellationToken), JsonOptions);
            if (data is null)
            {
                continue;
            }

            var retrievedOn = DateTime.SpecifyKind(DateTime.Parse(data.RetrievedOn), DateTimeKind.Utc);

            foreach (var item in data.Options)
            {
                var key = (data.Origin, data.DestinationDistrict, item.Mode, item.ServiceName);
                var isNew = !existing.TryGetValue(key, out var option);
                if (isNew)
                {
                    option = new TransportOption { Id = Guid.NewGuid() };
                    context.TransportOptions.Add(option);
                }
                else if (option!.DataRetrievedOn >= retrievedOn)
                {
                    continue;   // already current
                }

                option!.Mode = item.Mode;
                option.OriginName = data.Origin;
                option.DestinationDistrict = data.DestinationDistrict;
                option.Operator = item.Operator;
                option.ServiceName = item.ServiceName;
                option.ServiceClasses = item.ServiceClasses;
                option.Schedule = item.Schedule;
                option.DurationText = item.DurationText;
                option.FareBdt = item.FareBdt;
                option.FareNote = item.FareNote;
                option.BookingUrl = item.BookingUrl;
                option.Notes = item.Notes;
                option.SourceUrl = item.SourceUrl;
                option.VerificationStatus = item.VerificationStatus;
                option.UnverifiedFields = item.UnverifiedFields is { Count: > 0 } ? string.Join("; ", item.UnverifiedFields) : null;
                option.DataRetrievedOn = retrievedOn;
                option.IsActive = true;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed class TransportDataFile
    {
        public string Origin { get; set; } = string.Empty;
        public string DestinationDistrict { get; set; } = string.Empty;
        public string RetrievedOn { get; set; } = string.Empty;
        public List<TransportDataItem> Options { get; set; } = new();
    }

    private sealed class TransportDataItem
    {
        public string Mode { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceClasses { get; set; }
        public string? Schedule { get; set; }
        public string? DurationText { get; set; }
        public decimal? FareBdt { get; set; }
        public string? FareNote { get; set; }
        public string? BookingUrl { get; set; }
        public string? Notes { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = "SecondarySource";
        public List<string>? UnverifiedFields { get; set; }
    }
}
