using System.Text.Json;

namespace ShilpoHubBD.Application.DTOs.ProductSearch;

// ---- Lookups (product types and materials share one shape) --------------------------------------------------

public class LookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Create/update for a product type or a material. <c>Slug</c> is generated from the name when empty.</summary>
public class SaveLookupItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string? Slug { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ---- Producer-confirmed attributes --------------------------------------------------------------------------

public class ProductAttributesDto
{
    public Guid ProductId { get; set; }
    public bool Exists { get; set; }

    public Guid? ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public List<Guid> MaterialIds { get; set; } = new();
    public List<string> MaterialNames { get; set; } = new();

    public List<string> Tags { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public List<string> Colors { get; set; } = new();

    public string? CraftTechnique { get; set; }
    public string? ProductionMethod { get; set; }
    public string? DimensionsText { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightGrams { get; set; }
    public bool MadeToOrder { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? CareInstructions { get; set; }

    public string Source { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}

public class SaveProductAttributesRequest
{
    public Guid? ProductTypeId { get; set; }
    public List<Guid> MaterialIds { get; set; } = new();

    public List<string> Tags { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public List<string> Colors { get; set; } = new();

    public string? CraftTechnique { get; set; }
    public string? ProductionMethod { get; set; }
    public string? DimensionsText { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightGrams { get; set; }
    public bool MadeToOrder { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? CareInstructions { get; set; }
}

// ---- AI suggestions (never final until the producer confirms) ------------------------------------------------

public class AttributeSuggestionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    /// <summary>What the AI proposed. Purely advisory.</summary>
    public JsonElement Suggested { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>The producer's reviewed (and possibly edited) values; these, not the raw suggestion, are what gets saved.</summary>
public class ConfirmAttributeSuggestionRequest
{
    public SaveProductAttributesRequest Attributes { get; set; } = new();
}

/// <summary>Posted by the AI service (internal key). Stored as a pending suggestion only.</summary>
public class SubmitAttributeSuggestionRequest
{
    public Guid ProductId { get; set; }
    public string Model { get; set; } = string.Empty;
    public JsonElement Suggested { get; set; }
}

/// <summary>The product's own text sent to the AI service so it can suggest attributes (no ids, no prices, no personal data).</summary>
public record ProductForSuggestion(string Name, string Description, string? Story, string? Category, string? District);

public class GeneratedAttributeSuggestion
{
    public JsonElement Suggested { get; set; }
    public string Model { get; set; } = string.Empty;
}

// ---- Index feed (consumed by the Python sync worker; carries no database access) ----------------------------

public class ProductIndexChunkDto
{
    /// <summary>overview | story</summary>
    public string Kind { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class ProductIndexItemDto
{
    public Guid ProductId { get; set; }

    /// <summary>State version this item was built from; echoed back in the ack.</summary>
    public int Version { get; set; }

    /// <summary>upsert = re-embed and store; payload = only refresh the filterable snapshot; delete = remove the vectors.</summary>
    public string Action { get; set; } = "upsert";
    public string? TextHash { get; set; }
    public List<ProductIndexChunkDto> Chunks { get; set; } = new();
    public Dictionary<string, object?> Payload { get; set; } = new();
}

public class ProductIndexBatchDto
{
    public List<ProductIndexItemDto> Items { get; set; } = new();
    public int PendingTotal { get; set; }
}

public class ProductIndexAckItem
{
    public Guid ProductId { get; set; }
    public int Version { get; set; }
    public bool Success { get; set; }
    public string? TextHash { get; set; }
    public string? EmbeddingModel { get; set; }
    public string? Error { get; set; }
}

public class ProductIndexAckRequest
{
    public List<ProductIndexAckItem> Items { get; set; } = new();
}
