using System.Text.Json;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Application.Services.ProductSearch;

/// <summary>
/// Descriptive product attributes. Only the producer (or an admin) writes the final values; AI output is stored as a
/// <see cref="ProductAttributeSuggestion"/> and only becomes final when the producer reviews and confirms it.
/// </summary>
public class ProductAttributesService : IProductAttributesService
{
    private readonly IProductAttributesRepository _repository;
    private readonly IProductAttributeSuggester _suggester;

    public ProductAttributesService(IProductAttributesRepository repository, IProductAttributeSuggester suggester)
    {
        _repository = repository;
        _suggester = suggester;
    }

    public async Task<ProductAttributesDto> GetAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);
        return ToDto(await LoadProductAsync(productId, cancellationToken));
    }

    public async Task<ProductAttributesDto> SaveAsync(Guid productId, SaveProductAttributesRequest request, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);
        return await ApplyAsync(productId, request, isAdmin ? AttributesSources.Admin : AttributesSources.Producer, cancellationToken);
    }

    public async Task<AttributeSuggestionDto?> GetPendingSuggestionAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);
        var suggestion = await _repository.GetPendingSuggestionAsync(productId, cancellationToken);
        return suggestion is null ? null : ToDto(suggestion);
    }

    public async Task<ProductAttributesDto> ConfirmSuggestionAsync(
        Guid productId, Guid suggestionId, ConfirmAttributeSuggestionRequest request, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);
        var suggestion = await LoadSuggestionAsync(productId, suggestionId, cancellationToken);

        // The saved values are the producer's reviewed request, never the raw AI payload.
        var result = await ApplyAsync(productId, request.Attributes, isAdmin ? AttributesSources.Admin : AttributesSources.Producer, cancellationToken, save: false);
        suggestion.Status = SuggestionStatus.Confirmed;
        suggestion.ReviewedAt = DateTime.UtcNow;
        suggestion.ReviewedByUserId = userId;
        await _repository.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task DismissSuggestionAsync(Guid productId, Guid suggestionId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);
        var suggestion = await LoadSuggestionAsync(productId, suggestionId, cancellationToken);
        suggestion.Status = SuggestionStatus.Dismissed;
        suggestion.ReviewedAt = DateTime.UtcNow;
        suggestion.ReviewedByUserId = userId;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AttributeSuggestionDto> GenerateSuggestionAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(productId, userId, isAdmin, cancellationToken);

        // A double click (or a retry) reuses the suggestion that was just made instead of paying for another AI call.
        var recent = await _repository.GetPendingSuggestionAsync(productId, cancellationToken);
        if (recent is not null && DateTime.UtcNow - recent.CreatedAt < TimeSpan.FromSeconds(20))
        {
            return ToDto(recent);
        }

        var product = await LoadProductAsync(productId, cancellationToken);
        var generated = await _suggester.SuggestAsync(
            new ProductForSuggestion(product.Name, product.Description, product.Story, product.Category?.Name, product.District?.Name), cancellationToken)
            ?? throw new ConflictException("AI suggestions are unavailable right now. You can fill in the details yourself, or try again in a minute.");

        return await SubmitSuggestionAsync(new SubmitAttributeSuggestionRequest { ProductId = productId, Model = generated.Model, Suggested = generated.Suggested }, cancellationToken);
    }

    public async Task<AttributeSuggestionDto> SubmitSuggestionAsync(SubmitAttributeSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (await _repository.GetProducerIdAsync(request.ProductId, cancellationToken) is null)
        {
            throw new NotFoundException("Product not found.");
        }

        // A newer proposal replaces any older unreviewed one.
        foreach (var old in await _repository.GetPendingSuggestionsForProductAsync(request.ProductId, cancellationToken))
        {
            old.Status = SuggestionStatus.Dismissed;
            old.ReviewedAt = DateTime.UtcNow;
        }

        var suggestion = new ProductAttributeSuggestion
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            PayloadJson = request.Suggested.GetRawText(),
            Model = request.Model.Trim(),
            Status = SuggestionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        await _repository.AddSuggestionAsync(suggestion, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(suggestion);
    }

    // ---- helpers ----
    private async Task EnsureAccessAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var producerId = await _repository.GetProducerIdAsync(productId, cancellationToken) ?? throw new NotFoundException("Product not found.");
        if (!isAdmin && producerId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to change this product.");
        }
    }

    private async Task<Product> LoadProductAsync(Guid productId, CancellationToken cancellationToken)
        => await _repository.GetProductWithAttributesAsync(productId, cancellationToken) ?? throw new NotFoundException("Product not found.");

    private async Task<ProductAttributeSuggestion> LoadSuggestionAsync(Guid productId, Guid suggestionId, CancellationToken cancellationToken)
    {
        var suggestion = await _repository.GetSuggestionAsync(suggestionId, cancellationToken);
        if (suggestion is null || suggestion.ProductId != productId)
        {
            throw new NotFoundException("Suggestion not found.");
        }

        return suggestion.Status == SuggestionStatus.Pending ? suggestion : throw new ConflictException("This suggestion has already been reviewed.");
    }

    private async Task<ProductAttributesDto> ApplyAsync(Guid productId, SaveProductAttributesRequest request, string source, CancellationToken cancellationToken, bool save = true)
    {
        var product = await LoadProductAsync(productId, cancellationToken);
        var now = DateTime.UtcNow;

        if (request.ProductTypeId is { } typeId)
        {
            product.ProductType = await _repository.GetActiveTypeAsync(typeId, cancellationToken) ?? throw new ConflictException("Product type not found or inactive.");
            product.ProductTypeId = typeId;
        }
        else
        {
            product.ProductType = null;
            product.ProductTypeId = null;
        }

        var materialIds = request.MaterialIds.Distinct().ToList();
        var materials = materialIds.Count == 0 ? new List<Material>() : await _repository.GetActiveMaterialsAsync(materialIds, cancellationToken);
        if (materials.Count != materialIds.Count)
        {
            throw new ConflictException("One or more materials were not found or are inactive.");
        }

        // Diff instead of clear-and-re-add so an unchanged (product, material) pair is never deleted and inserted in one save.
        var wanted = materials.Select(m => m.Id).ToHashSet();
        foreach (var stale in product.Materials.Where(pm => !wanted.Contains(pm.MaterialId)).ToList())
        {
            product.Materials.Remove(stale);
        }

        var present = product.Materials.Select(pm => pm.MaterialId).ToHashSet();
        foreach (var material in materials.Where(m => !present.Contains(m.Id)))
        {
            product.Materials.Add(new ProductMaterial { ProductId = productId, MaterialId = material.Id, Material = material });
        }

        var attributes = product.Attributes;
        if (attributes is null)
        {
            attributes = new ProductAttributes { ProductId = productId, CreatedAt = now };
            product.Attributes = attributes;
            await _repository.AddAttributesAsync(attributes, cancellationToken);
        }

        attributes.Tags = CleanList(request.Tags);
        attributes.Keywords = CleanList(request.Keywords);
        attributes.Occasions = CleanList(request.Occasions);
        attributes.Colors = CleanList(request.Colors);
        attributes.CraftTechnique = Clean(request.CraftTechnique);
        attributes.ProductionMethod = Clean(request.ProductionMethod);
        attributes.DimensionsText = Clean(request.DimensionsText);
        attributes.LengthCm = request.LengthCm;
        attributes.WidthCm = request.WidthCm;
        attributes.HeightCm = request.HeightCm;
        attributes.WeightGrams = request.WeightGrams;
        attributes.MadeToOrder = request.MadeToOrder;
        attributes.LeadTimeDays = request.MadeToOrder ? request.LeadTimeDays : null;
        attributes.CareInstructions = Clean(request.CareInstructions);
        attributes.Source = source;
        attributes.UpdatedAt = now;

        if (save)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(product);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> CleanList(IEnumerable<string>? values)
        => (values ?? Enumerable.Empty<string>()).Select(v => v?.Trim() ?? string.Empty).Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static ProductAttributesDto ToDto(Product product)
    {
        var a = product.Attributes;
        return new ProductAttributesDto
        {
            ProductId = product.Id,
            Exists = a is not null,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType?.Name,
            MaterialIds = product.Materials.Select(m => m.MaterialId).ToList(),
            MaterialNames = product.Materials.Select(m => m.Material?.Name ?? string.Empty).Where(n => n.Length > 0).ToList(),
            Tags = a?.Tags ?? new(),
            Keywords = a?.Keywords ?? new(),
            Occasions = a?.Occasions ?? new(),
            Colors = a?.Colors ?? new(),
            CraftTechnique = a?.CraftTechnique,
            ProductionMethod = a?.ProductionMethod,
            DimensionsText = a?.DimensionsText,
            LengthCm = a?.LengthCm,
            WidthCm = a?.WidthCm,
            HeightCm = a?.HeightCm,
            WeightGrams = a?.WeightGrams,
            MadeToOrder = a?.MadeToOrder ?? false,
            LeadTimeDays = a?.LeadTimeDays,
            CareInstructions = a?.CareInstructions,
            Source = a?.Source ?? string.Empty,
            UpdatedAt = a?.UpdatedAt,
        };
    }

    private static AttributeSuggestionDto ToDto(ProductAttributeSuggestion s) => new()
    {
        Id = s.Id,
        ProductId = s.ProductId,
        Suggested = JsonDocument.Parse(s.PayloadJson).RootElement.Clone(),
        Model = s.Model,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt,
    };
}
