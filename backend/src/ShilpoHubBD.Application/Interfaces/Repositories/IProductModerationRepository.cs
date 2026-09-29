using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProductModerationRepository
{
    /// <summary>Loads (creating if needed) the moderation state for a product, with its Product navigation
    /// loaded (for producer id/name when building a warning or admin alert).</summary>
    Task<ProductModerationState> GetOrCreateStateAsync(Guid productId, CancellationToken cancellationToken);

    /// <summary>Database-only fallback candidate source when the RAG similarity search is unavailable: the
    /// most recent OTHER reviews for the same product that already have a negative AI analysis.</summary>
    Task<List<Guid>> GetRecentNegativeReviewIdsForProductAsync(Guid productId, Guid excludeReviewId, int limit, CancellationToken cancellationToken);

    /// <summary>Hydrates candidate review ids with their real text and AI analysis from Postgres — the vector
    /// search is only ever trusted for ids/relevance, never for the review's own content.</summary>
    Task<Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)>> GetReviewsWithAnalysisByIdsAsync(
        IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken);

    Task<List<ProductModerationEvent>> GetRecentEventsAsync(Guid productId, int limit, CancellationToken cancellationToken);

    /// <summary>How many reviews for this product have ever been flagged negative by AI moderation (Part 1),
    /// out of the product's total review count (already on <c>Product.ReviewCount</c>).</summary>
    Task<int> CountNegativeReviewsForProductAsync(Guid productId, CancellationToken cancellationToken);

    Task<List<ProducerModerationWarning>> GetWarningsForProductAsync(Guid productId, int limit, CancellationToken cancellationToken);

    /// <summary>Admin moderation list: every open product-complaint moderation case, joined live from
    /// <c>MonitoringFlag</c> and <c>ProductModerationState</c> (never a frozen snapshot).</summary>
    Task<(List<ProductModerationCaseRow> Items, int TotalCount)> GetCasesAsync(
        ProductModerationRiskState? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken);

    Task AddEventAsync(ProductModerationEvent moderationEvent, CancellationToken cancellationToken);
    Task AddWarningAsync(ProducerModerationWarning warning, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>One joined row behind <see cref="GetCasesAsync"/>, before it's mapped to the API DTO.</summary>
public record ProductModerationCaseRow(
    Guid FlagId, Guid ProductId, string ProductName, Guid ProducerId, string ProducerName,
    ProductModerationRiskState RiskState, int SimilarComplaintCount, int HighSeverityComplaintCount,
    int ProducerWarningCount, string ModerationStatus, DateTime DetectedAt);
