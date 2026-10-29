using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Application.DTOs.PerformingArts;

public class PerformingArtListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public PerformingArtCategory Category { get; set; }
    public string SummaryEn { get; set; } = string.Empty;
    public string? SummaryBn { get; set; }
    public string? Division { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Communities { get; set; }
    public bool IsFeatured { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? CoverImageCredit { get; set; }
    public int SourceCount { get; set; }
    public PerformingArtStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PerformingArtSourceDto
{
    public Guid Id { get; set; }
    public PerformingArtSourceType SourceType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Url { get; set; }
    public DateTime? AccessedOn { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}

public class PerformingArtMediaDto
{
    public Guid Id { get; set; }
    public PerformingArtMediaType MediaType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? CaptionEn { get; set; }
    public string? CaptionBn { get; set; }
    public string Credit { get; set; } = string.Empty;
    public string? License { get; set; }
    public string? SourceUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class PerformingArtDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? AlternateNames { get; set; }
    public PerformingArtCategory Category { get; set; }
    public string SummaryEn { get; set; } = string.Empty;
    public string? SummaryBn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionBn { get; set; }
    public string? RegionalContextEn { get; set; }
    public string? RegionalContextBn { get; set; }
    public string? Division { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Upazila { get; set; }
    public string? Communities { get; set; }
    public string? Artists { get; set; }
    public string? Instruments { get; set; }
    public string? ViabilityNote { get; set; }
    public string? InventoryReference { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public PerformingArtStatus Status { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PerformingArtSourceDto> Sources { get; set; } = new();
    public List<PerformingArtMediaDto> Media { get; set; } = new();
}

public class SavePerformingArtSourceRequest
{
    public PerformingArtSourceType SourceType { get; set; } = PerformingArtSourceType.Website;
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Url { get; set; }
    public DateTime? AccessedOn { get; set; }
    public string? Notes { get; set; }
}

public class SavePerformingArtMediaRequest
{
    public PerformingArtMediaType MediaType { get; set; } = PerformingArtMediaType.Image;
    public string Url { get; set; } = string.Empty;
    public string? CaptionEn { get; set; }
    public string? CaptionBn { get; set; }
    public string Credit { get; set; } = string.Empty;
    public string? License { get; set; }
    public string? SourceUrl { get; set; }
}

/// <summary>Create and update body. Sources and media are replaced wholesale, in the given order. Status is never
/// set here — it only changes through the workflow endpoints.</summary>
public class SavePerformingArtRequest
{
    public string Slug { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? AlternateNames { get; set; }
    public PerformingArtCategory Category { get; set; }
    public string SummaryEn { get; set; } = string.Empty;
    public string? SummaryBn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionBn { get; set; }
    public string? RegionalContextEn { get; set; }
    public string? RegionalContextBn { get; set; }
    public string? Division { get; set; }
    public Guid? DistrictId { get; set; }
    public string? Upazila { get; set; }
    public string? Communities { get; set; }
    public string? Artists { get; set; }
    public string? Instruments { get; set; }
    public string? ViabilityNote { get; set; }
    public string? InventoryReference { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public List<SavePerformingArtSourceRequest> Sources { get; set; } = new();
    public List<SavePerformingArtMediaRequest> Media { get; set; } = new();
}

public class PerformingArtWorkflowRequest
{
    public string? Note { get; set; }
}

public class PerformingArtQueryParameters
{
    /// <summary>Matched against English and Bangla titles, alternate names, summaries, communities, artists,
    /// instruments, upazila, district and division.</summary>
    public string? Search { get; set; }
    public PerformingArtCategory? Category { get; set; }
    public string? Division { get; set; }
    public Guid? DistrictId { get; set; }
    public bool? Featured { get; set; }

    /// <summary>Admin listing only; ignored on public endpoints (which always return Published).</summary>
    public PerformingArtStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public record PerformingArtCategoryFacet(PerformingArtCategory Category, int Count);
public record PerformingArtDivisionFacet(string Division, int Count);
public record PerformingArtDistrictFacet(Guid Id, string Name, string Division, int Count);

public class PerformingArtFacetsDto
{
    public List<PerformingArtCategoryFacet> Categories { get; set; } = new();
    public List<PerformingArtDivisionFacet> Divisions { get; set; } = new();
    public List<PerformingArtDistrictFacet> Districts { get; set; } = new();
}
