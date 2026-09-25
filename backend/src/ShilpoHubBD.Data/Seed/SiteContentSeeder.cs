using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Cms;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Seed;

// First-run content for the admin-managed public copy (About page, footer, travel resources) and the craft
// heritage articles, imported from Seed/ContentData/*.json. A table is only seeded while it is EMPTY, so once an
// admin edits or deletes rows in Admin, restarts never bring the seed content back.
public static class SiteContentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Seed", "ContentData");
        var now = DateTime.UtcNow;

        var siteFile = Path.Combine(folder, "site-content.json");
        if (File.Exists(siteFile) && !await context.SiteContentItems.AnyAsync(cancellationToken))
        {
            var items = JsonSerializer.Deserialize<List<SiteContentSeed>>(await File.ReadAllTextAsync(siteFile, cancellationToken), JsonOptions) ?? new();
            context.SiteContentItems.AddRange(items.Select(i => new SiteContentItem
            {
                Id = Guid.NewGuid(),
                Group = i.Group,
                Title = i.Title,
                Subtitle = i.Subtitle,
                Body = i.Body,
                LinkUrl = i.LinkUrl,
                Extra = i.Extra,
                DisplayOrder = i.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            }));
            await context.SaveChangesAsync(cancellationToken);
        }

        var craftFile = Path.Combine(folder, "craft-heritage.json");
        if (File.Exists(craftFile) && !await context.CraftHeritageEntries.AnyAsync(cancellationToken))
        {
            var entries = JsonSerializer.Deserialize<List<CraftHeritageSeed>>(await File.ReadAllTextAsync(craftFile, cancellationToken), JsonOptions) ?? new();
            context.CraftHeritageEntries.AddRange(entries.Select(e => new CraftHeritageEntry
            {
                Id = Guid.NewGuid(),
                Slug = e.Slug,
                Name = e.Name,
                Aliases = e.Aliases,
                Region = e.Region,
                Type = e.Type,
                GiName = e.GiName,
                Unesco = e.Unesco,
                Summary = e.Summary,
                History = e.History,
                Materials = e.Materials,
                Process = e.Process,
                Products = e.Products,
                Story = e.Story,
                Visit = e.Visit,
                Sources = e.Sources,
                DisplayOrder = e.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            }));
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed record SiteContentSeed(string Group, string Title, string? Subtitle, string? Body, string? LinkUrl, string? Extra, int DisplayOrder);

    private sealed record CraftHeritageSeed(
        string Slug, string Name, string? Aliases, string Region, string Type, string? GiName, string? Unesco, string Summary,
        string? History, string? Materials, string? Process, string? Products, string? Story, string? Visit, string? Sources, int DisplayOrder);
}
