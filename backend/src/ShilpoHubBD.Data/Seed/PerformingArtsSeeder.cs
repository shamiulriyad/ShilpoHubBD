using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Data.Seed;

// First-run performing-arts records from Seed/ContentData/performing-arts.json, transcribed from the Cultural Heritage
// Inventory of Bangladesh (heritagehub.gov.bd). They are seeded as InReview — never Published — so an editor checks
// each one against the official record before it reaches the public site or the AI assistant. Seeds only while the
// table is empty, and skips quietly if the AddPerformingArtsModule migration has not been applied yet.
public static class PerformingArtsSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Seed", "ContentData", "performing-arts.json");
        if (!File.Exists(file))
        {
            return;
        }

        try
        {
            if (await context.PerformingArtRecords.AnyAsync(cancellationToken))
            {
                return;
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return;
        }

        var seeds = JsonSerializer.Deserialize<List<RecordSeed>>(await File.ReadAllTextAsync(file, cancellationToken), JsonOptions) ?? new();
        var names = seeds.Select(s => s.DistrictName).Where(n => n != null).ToList();
        var districts = await context.Districts.Where(d => names.Contains(d.Name)).ToDictionaryAsync(d => d.Name, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var s in seeds)
        {
            var district = s.DistrictName != null && districts.TryGetValue(s.DistrictName, out var d) ? d : null;
            var record = new PerformingArtRecord
            {
                Id = Guid.NewGuid(),
                Slug = s.Slug,
                TitleEn = s.TitleEn,
                TitleBn = s.TitleBn,
                AlternateNames = s.AlternateNames,
                Category = s.Category,
                SummaryEn = s.SummaryEn,
                DescriptionEn = s.DescriptionEn,
                RegionalContextEn = s.RegionalContextEn,
                DistrictId = district?.Id,
                Division = district?.Division,
                Upazila = s.Upazila,
                Communities = s.Communities,
                Instruments = s.Instruments,
                ViabilityNote = s.ViabilityNote,
                InventoryReference = s.InventoryReference,
                DisplayOrder = s.DisplayOrder,
                Status = PerformingArtStatus.InReview,
                SubmittedAt = now,
                ReviewNote = "Imported from the Cultural Heritage Inventory of Bangladesh. Check every field against the official record before publishing; no Bangla description or media has been added yet.",
                CreatedAt = now,
                UpdatedAt = now,
            };
            foreach (var (src, i) in s.Sources.Select((src, i) => (src, i)))
            {
                record.Sources.Add(new PerformingArtSource
                {
                    Id = Guid.NewGuid(), RecordId = record.Id, SourceType = src.SourceType, Title = src.Title, Publisher = src.Publisher,
                    Url = src.Url, Notes = src.Notes, AccessedOn = src.Url != null ? new DateTime(2026, 10, 30, 0, 0, 0, DateTimeKind.Utc) : null,
                    DisplayOrder = i,
                });
            }

            context.PerformingArtRecords.Add(record);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record SourceSeed(PerformingArtSourceType SourceType, string Title, string? Publisher, string? Url, string? Notes);

    private sealed record RecordSeed(
        string Slug, string TitleEn, string? TitleBn, string? AlternateNames, PerformingArtCategory Category, string? DistrictName,
        string? Upazila, string? Communities, string? Instruments, string? ViabilityNote, string? InventoryReference, string SummaryEn,
        string? DescriptionEn, string? RegionalContextEn, int DisplayOrder, List<SourceSeed> Sources);
}
