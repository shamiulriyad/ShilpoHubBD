using System.Security.Cryptography;
using System.Text;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Application.Services.ProductSearch;

/// <summary>
/// Feeds the product vector index. PostgreSQL stays the source of truth: this builds, per product, the text to embed
/// (semantic fields only) and a filterable payload snapshot (price, stock, rating... for pre-filtering). The Python
/// worker consumes it over HTTP, so the AI side never holds database credentials.
/// </summary>
public class ProductIndexService : IProductIndexService
{
    public const int MaxAttempts = 5;
    private const int MaxBatch = 100;

    private readonly IProductIndexRepository _repository;

    public ProductIndexService(IProductIndexRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductIndexBatchDto> GetPendingAsync(int limit, CancellationToken cancellationToken)
    {
        var states = await _repository.GetPendingAsync(Math.Clamp(limit, 1, MaxBatch), MaxAttempts, cancellationToken);
        var batch = new ProductIndexBatchDto { PendingTotal = await _repository.CountPendingAsync(MaxAttempts, cancellationToken) };
        if (states.Count == 0)
        {
            return batch;
        }

        var ids = states.Select(s => s.ProductId).ToList();
        var products = await _repository.GetProductsForIndexAsync(ids, cancellationToken);
        var sources = await _repository.GetMaterialSourcesAsync(ids, cancellationToken);
        var workshops = await _repository.GetWorkshopNamesAsync(products.Values.Select(p => p.ProducerId).Distinct().ToList(), cancellationToken);
        var heritage = await _repository.GetCraftHeritageBySlugAsync(products.Values.Select(p => p.Category.Slug).Distinct().ToList(), cancellationToken);

        foreach (var state in states)
        {
            if (state.Status == IndexStatuses.Deleted || !products.TryGetValue(state.ProductId, out var product))
            {
                batch.Items.Add(new ProductIndexItemDto { ProductId = state.ProductId, Version = state.Version, Action = "delete" });
                continue;
            }

            heritage.TryGetValue(product.Category.Slug, out var craft);
            sources.TryGetValue(product.Id, out var materialSources);
            workshops.TryGetValue(product.ProducerId, out var workshop);

            var chunks = BuildChunks(product, craft, materialSources ?? new List<string>(), workshop);
            var hash = Hash(chunks);
            batch.Items.Add(new ProductIndexItemDto
            {
                ProductId = product.Id,
                Version = state.Version,
                // Same embedded text as last time: refresh only the filterable snapshot, no embedding call.
                Action = hash == state.SearchTextHash ? "payload" : "upsert",
                TextHash = hash,
                Chunks = chunks,
                Payload = BuildPayload(product, craft, workshop),
            });
        }

        return batch;
    }

    public async Task AckAsync(ProductIndexAckRequest request, CancellationToken cancellationToken)
    {
        var states = await _repository.GetStatesAsync(request.Items.Select(i => i.ProductId).Distinct().ToList(), cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var item in request.Items)
        {
            if (!states.TryGetValue(item.ProductId, out var state))
            {
                continue;
            }

            if (!item.Success)
            {
                state.Status = IndexStatuses.Failed;
                state.AttemptCount++;
                state.LastError = item.Error is { Length: > 1000 } ? item.Error[..1000] : item.Error;
                state.UpdatedAt = now;
                continue;
            }

            // Changed again while the worker was busy: stay Dirty so the newer version is picked up next round.
            if (state.Version != item.Version)
            {
                continue;
            }

            if (state.Status == IndexStatuses.Deleted)
            {
                _repository.RemoveState(state);
                continue;
            }

            state.Status = IndexStatuses.Synced;
            state.SearchTextHash = item.TextHash ?? state.SearchTextHash;
            state.EmbeddingModel = item.EmbeddingModel ?? state.EmbeddingModel;
            state.IndexedAt = now;
            state.AttemptCount = 0;
            state.LastError = null;
            state.UpdatedAt = now;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<int> RequeueAllAsync(CancellationToken cancellationToken) => _repository.RequeueAllAsync(cancellationToken);

    public Task<Dictionary<string, int>> GetStatsAsync(CancellationToken cancellationToken) => _repository.CountByStatusAsync(cancellationToken);

    // ---- document building -------------------------------------------------------------------------------------

    private static List<ProductIndexChunkDto> BuildChunks(Product p, CraftHeritageEntry? craft, List<string> materialSources, string? workshop)
    {
        var a = p.Attributes;
        var materials = p.Materials.Select(m => m.Material).Where(m => m is not null).ToList();
        var overview = new List<string>();

        var typeName = p.ProductType is null ? null : Join(p.ProductType.Name, p.ProductType.NameBn);
        overview.Add($"{p.Name}.");
        overview.Add(typeName is null
            ? $"From the craft {p.Category.Name}, {p.District.Name} ({p.District.Division} division)."
            : $"{typeName} from the craft {p.Category.Name}, {p.District.Name} ({p.District.Division} division).");
        if (materials.Count > 0) overview.Add($"Made of {string.Join(", ", materials.Select(m => Join(m.Name, m.NameBn)))}.");
        if (a?.ProductionMethod is { } method) overview.Add($"{method}.");
        if (a is { Occasions.Count: > 0 }) overview.Add($"Suits {string.Join(", ", a.Occasions)}.");
        if (a is { Colors.Count: > 0 }) overview.Add($"Colours: {string.Join(", ", a.Colors)}.");
        if (a is { Tags.Count: > 0 }) overview.Add($"Tags: {string.Join(", ", a.Tags)}.");
        if (a is { Keywords.Count: > 0 }) overview.Add($"Also known as {string.Join(", ", a.Keywords)}.");
        if (craft is not null)
        {
            var aliases = (craft.Aliases ?? string.Empty).Trim();
            // Only the alternative names are embedded. GI / UNESCO status is the same for every product of a craft, so putting it
            // in the text would make all of them near-identical; it is exposed as the is_gi / is_unesco payload filters instead.
            if (aliases.Length > 0) overview.Add($"The craft is also called {aliases}.");
        }

        overview.Add($"Made by {(string.IsNullOrWhiteSpace(workshop) ? p.Producer.FullName : $"{workshop} ({p.Producer.FullName})")}.");

        var story = new List<string> { $"{p.Name}.", p.Description.Trim() };
        if (!string.IsNullOrWhiteSpace(p.Story)) story.Add(p.Story.Trim());
        if (!string.IsNullOrWhiteSpace(a?.CraftTechnique)) story.Add($"Technique: {a!.CraftTechnique}.");
        if (materialSources.Count > 0) story.Add($"Material provenance: {string.Join("; ", materialSources)}.");
        if (!string.IsNullOrWhiteSpace(a?.CareInstructions)) story.Add($"Care: {a!.CareInstructions}.");

        return new List<ProductIndexChunkDto>
        {
            new() { Kind = "overview", Text = string.Join(' ', overview) },
            new() { Kind = "story", Text = string.Join(' ', story) },
        };
    }

    /// <summary>Filterable snapshot. Price, stock and rating here are only for pre-filtering; PostgreSQL is re-read for the answer.</summary>
    private static Dictionary<string, object?> BuildPayload(Product p, CraftHeritageEntry? craft, string? workshop)
    {
        var a = p.Attributes;
        var inStock = p.Stock > 0 || p.Variants.Any(v => v.IsActive && v.Stock > 0);
        var primaryImage = p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault();

        return new Dictionary<string, object?>
        {
            ["product_id"] = p.Id,
            ["slug"] = p.Slug,
            ["name"] = p.Name,
            ["category_id"] = p.CategoryId,
            ["category_slug"] = p.Category.Slug,
            ["category_name"] = p.Category.Name,
            ["product_type"] = p.ProductType?.Slug,
            ["product_type_name"] = p.ProductType?.Name,
            ["materials"] = p.Materials.Select(m => m.Material.Slug).ToList(),
            ["tags"] = a?.Tags ?? new List<string>(),
            ["occasions"] = a?.Occasions ?? new List<string>(),
            ["colors"] = a?.Colors ?? new List<string>(),
            ["district_id"] = p.DistrictId,
            ["district"] = p.District.Name,
            ["division"] = p.District.Division,
            ["producer_id"] = p.ProducerId,
            ["producer_name"] = p.Producer.FullName,
            ["workshop_name"] = workshop,
            ["handmade_status"] = p.HandmadeVerificationStatus.ToString(),
            ["is_handmade_verified"] = p.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified,
            ["is_gi"] = !string.IsNullOrWhiteSpace(craft?.GiName),
            ["is_unesco"] = !string.IsNullOrWhiteSpace(craft?.Unesco),
            ["made_to_order"] = a?.MadeToOrder ?? false,
            ["is_public"] = p.IsActive && p.ApprovalStatus == ProductApprovalStatus.Approved,
            ["in_stock"] = inStock,
            ["price"] = p.Price,
            ["discount_price"] = p.DiscountPrice,
            ["effective_price"] = p.DiscountPrice ?? p.Price,
            ["currency"] = "BDT",
            ["rating"] = p.AverageRating,
            ["bayesian_rating"] = p.BayesianRating,
            ["review_count"] = p.ReviewCount,
            ["image_url"] = primaryImage,
            ["updated_at"] = p.UpdatedAt,
        };
    }

    private static string Join(string name, string? nameBn) => string.IsNullOrWhiteSpace(nameBn) ? name : $"{name} ({nameBn})";

    private static string Hash(List<ProductIndexChunkDto> chunks)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', chunks.Select(c => $"{c.Kind}\u001e{c.Text}"))))).ToLowerInvariant();
}
