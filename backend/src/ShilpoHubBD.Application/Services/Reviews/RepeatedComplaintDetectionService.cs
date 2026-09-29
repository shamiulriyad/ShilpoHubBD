using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Services.Reviews;

/// <summary>
/// Retrieves historical reviews for the same product (via RAG, falling back to a plain database query),
/// asks Gemini (or its rule-based fallback) whether the new negative review repeats a known complaint, then
/// applies deterministic risk logic. Gemini only ever classifies; this class decides the product's risk state,
/// issues at most one producer reminder per threshold crossing, and hands the existing Governance Monitoring
/// dashboard the data it needs once a product reaches HighRisk — it never bans or restricts a product.
/// </summary>
public class RepeatedComplaintDetectionService : IRepeatedComplaintDetectionService
{
    private readonly IProductModerationRepository _repository;
    private readonly IMonitoringRepository _monitoringRepository;
    private readonly IReviewSimilarityProvider _similarityProvider;
    private readonly IRepeatedComplaintAIProvider _aiProvider;
    private readonly ProductModerationOptions _options;
    private readonly ILogger<RepeatedComplaintDetectionService> _logger;

    public RepeatedComplaintDetectionService(
        IProductModerationRepository repository,
        IMonitoringRepository monitoringRepository,
        IReviewSimilarityProvider similarityProvider,
        IRepeatedComplaintAIProvider aiProvider,
        IOptions<ProductModerationOptions> options,
        ILogger<RepeatedComplaintDetectionService> logger)
    {
        _repository = repository;
        _monitoringRepository = monitoringRepository;
        _similarityProvider = similarityProvider;
        _aiProvider = aiProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EvaluateAsync(Review review, ReviewAiAnalysis analysis, CancellationToken cancellationToken)
    {
        if (review.ProductId is not { } productId)
        {
            return;
        }

        var state = await _repository.GetOrCreateStateAsync(productId, cancellationToken);
        state.NegativeComplaintCount++;
        state.LastReviewId = review.Id;
        state.LastEvaluatedAt = DateTime.UtcNow;
        state.UpdatedAt = DateTime.UtcNow;

        var candidates = await GetHistoricalReviewsAsync(review, cancellationToken);
        if (candidates.Count == 0)
        {
            // Nothing to compare against yet (new product, or RAG and DB both came up empty): still record the
            // negative complaint, but there's no basis for a repeated-complaint verdict.
            await _repository.SaveChangesAsync(cancellationToken);
            return;
        }

        var comparison = await _aiProvider.CompareAsync(new RepeatedComplaintContext
        {
            // review.Product is not loaded on a freshly-created review; state.Product (loaded via
            // GetOrCreateStateAsync) is the real product record read from Postgres.
            ProductName = state.Product.Name,
            NewReviewText = review.Comment,
            NewComplaintType = analysis.ComplaintType,
            NewSeverity = analysis.Severity,
            HistoricalReviews = candidates,
        }, cancellationToken);

        if (comparison.Severity is ReviewSeverity.High or ReviewSeverity.Critical)
        {
            state.HighSeverityComplaintCount++;
        }

        if (comparison.IsRepeatedComplaint)
        {
            state.SimilarComplaintCount++;
            await _repository.AddEventAsync(NewEvent(
                state, review, ProductModerationEventType.RepeatedComplaintDetected, comparison, candidates), cancellationToken);
        }

        var previousState = state.RiskState;
        state.RiskState = ComputeRiskState(state, _options);

        if (previousState == ProductModerationRiskState.Normal && state.RiskState == ProductModerationRiskState.Warning)
        {
            await IssueProducerWarningAsync(state, comparison, cancellationToken);
            await _repository.AddEventAsync(NewEvent(
                state, review, ProductModerationEventType.WarningIssued, comparison, candidates), cancellationToken);
        }

        if (previousState != ProductModerationRiskState.HighRisk && state.RiskState == ProductModerationRiskState.HighRisk)
        {
            await _repository.AddEventAsync(NewEvent(
                state, review, ProductModerationEventType.HighRiskFlagged, comparison, candidates), cancellationToken);
            await CreateAdminAlertAsync(state, review, analysis, comparison, candidates, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- historical review retrieval (RAG, prioritizing the same product, with a DB fallback) -------------------

    private async Task<List<HistoricalReviewSnippet>> GetHistoricalReviewsAsync(Review review, CancellationToken cancellationToken)
    {
        var productId = review.ProductId!.Value;
        List<Guid> candidateIds;

        var ragMatches = await _similarityProvider.FindSimilarAsync(productId, review.Comment, _options.SimilarReviewLookback, cancellationToken);
        if (ragMatches is not null)
        {
            candidateIds = ragMatches.Select(m => m.ReviewId).Where(id => id != review.Id).Distinct().ToList();
        }
        else
        {
            // RAG unavailable: fall back to a plain database query, same failure-handling shape as the
            // existing product-search keyword fallback — real data, same product only, never fabricated.
            _logger.LogInformation("Review similarity search unavailable for product {ProductId}; using the database fallback.", productId);
            candidateIds = new List<Guid>();
        }

        if (candidateIds.Count == 0)
        {
            candidateIds = await _repository.GetRecentNegativeReviewIdsForProductAsync(
                productId, review.Id, _options.SimilarReviewLookback, cancellationToken);
        }

        if (candidateIds.Count == 0)
        {
            return new List<HistoricalReviewSnippet>();
        }

        var hydrated = await _repository.GetReviewsWithAnalysisByIdsAsync(candidateIds, cancellationToken);
        return hydrated.Values
            .Select(h => new HistoricalReviewSnippet
            {
                ReviewId = h.Review.Id,
                Text = h.Review.Comment,
                ComplaintType = h.Analysis?.ComplaintType ?? ReviewComplaintType.Other,
                Severity = h.Analysis?.Severity ?? ReviewSeverity.Low,
                SameProduct = h.Review.ProductId == productId,
            })
            // Same-product complaints are what this feature is about; anything else (should the RAG ever widen
            // the search) is kept only as lower-priority context.
            .OrderByDescending(c => c.SameProduct)
            .Take(_options.SimilarReviewLookback)
            .ToList();
    }

    // ---- risk logic (deterministic; Gemini never decides this) ---------------------------------------------------

    private static ProductModerationRiskState ComputeRiskState(ProductModerationState state, ProductModerationOptions options)
    {
        if (state.SimilarComplaintCount >= options.HighRiskSimilarComplaintThreshold
            || state.HighSeverityComplaintCount >= options.HighRiskSeverityComplaintThreshold)
        {
            return ProductModerationRiskState.HighRisk;
        }

        if (state.SimilarComplaintCount >= options.WarningThreshold)
        {
            return ProductModerationRiskState.Warning;
        }

        return ProductModerationRiskState.Normal;
    }

    private static ProductModerationEvent NewEvent(
        ProductModerationState state, Review review, ProductModerationEventType type,
        RepeatedComplaintResultDto comparison, List<HistoricalReviewSnippet> candidates) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = state.ProductId,
        ReviewId = review.Id,
        Type = type,
        ComplaintType = comparison.ComplaintType,
        Severity = comparison.Severity,
        Reason = comparison.Reason,
        Confidence = comparison.Confidence,
        IsAiGenerated = comparison.IsAiGenerated,
        SimilarReviewIds = candidates.Select(c => c.ReviewId).ToList(),
        CreatedAt = DateTime.UtcNow,
    };

    // ---- producer reminder (existing notification mechanism, fired once per threshold crossing) -------------------

    private async Task IssueProducerWarningAsync(ProductModerationState state, RepeatedComplaintResultDto comparison, CancellationToken cancellationToken)
    {
        state.ProducerWarningCount++;

        var message = $"Multiple customers have reported similar {comparison.ComplaintType} issues with this product " +
            $"({state.SimilarComplaintCount} similar complaints so far). Please review the product and take corrective action.";

        await _repository.AddWarningAsync(new ProducerModerationWarning
        {
            Id = Guid.NewGuid(),
            ProductId = state.ProductId,
            ProducerId = state.Product.ProducerId,
            ComplaintType = comparison.ComplaintType,
            SimilarComplaintCount = state.SimilarComplaintCount,
            Message = message,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }

    // ---- admin alert data (for the EXISTING Governance Monitoring / AI Moderation dashboard) -----------------------

    private async Task CreateAdminAlertAsync(
        ProductModerationState state, Review triggeringReview, ReviewAiAnalysis triggeringAnalysis,
        RepeatedComplaintResultDto comparison, List<HistoricalReviewSnippet> candidates, CancellationToken cancellationToken)
    {
        var dedupeKey = $"product-moderation:{state.ProductId}";
        var alreadyOpen = await _monitoringRepository.GetOpenFlagDedupeKeysAsync(new[] { dedupeKey }, cancellationToken);
        if (alreadyOpen.Contains(dedupeKey))
        {
            return;
        }

        var createdBy = await _monitoringRepository.GetAnySuperAdminUserIdAsync(cancellationToken);
        if (createdBy is not { } createdByUserId)
        {
            _logger.LogWarning("No SuperAdmin account exists yet; skipping the admin moderation alert for product {ProductId}.", state.ProductId);
            return;
        }

        var recentHistory = await _repository.GetRecentEventsAsync(state.ProductId, 10, cancellationToken);
        var now = DateTime.UtcNow;

        var evidence = new
        {
            productId = state.ProductId,
            productName = state.Product.Name,
            producerId = state.Product.ProducerId,
            riskState = state.RiskState.ToString(),
            negativeComplaintCount = state.NegativeComplaintCount,
            similarComplaintCount = state.SimilarComplaintCount,
            highSeverityComplaintCount = state.HighSeverityComplaintCount,
            producerWarningCount = state.ProducerWarningCount,
            triggeringReviewId = triggeringReview.Id,
            triggeringReviewComment = triggeringReview.Comment,
            triggeringComplaintType = triggeringAnalysis.ComplaintType.ToString(),
            triggeringSeverity = triggeringAnalysis.Severity.ToString(),
            similarHistoricalReviewIds = candidates.Select(c => c.ReviewId).ToList(),
            complaintType = comparison.ComplaintType.ToString(),
            severity = comparison.Severity.ToString(),
            isRepeatedComplaint = comparison.IsRepeatedComplaint,
            confidence = comparison.Confidence,
            aiExplanation = comparison.Reason,
            isAiGenerated = comparison.IsAiGenerated,
            moderationHistory = recentHistory.Select(e => new { e.Type, e.ComplaintType, e.Severity, e.ReviewId, e.CreatedAt }).ToList(),
        };

        var flag = new MonitoringFlag
        {
            Id = Guid.NewGuid(),
            FlagType = MonitoringFlagType.RepeatedProductComplaints,
            Severity = ToMonitoringSeverity(comparison.Severity),
            Status = MonitoringFlagStatus.Open,
            Source = MonitoringFlagSource.AutomatedScan,
            SubjectType = MonitoringSubjectType.Product,
            SubjectId = state.ProductId,
            SubjectLabel = state.Product.Name.Length > 200 ? state.Product.Name[..200] : state.Product.Name,
            Title = $"Repeated {comparison.ComplaintType} complaints on {state.Product.Name}",
            Description = comparison.Reason,
            EvidenceJson = JsonSerializer.Serialize(evidence),
            RiskScore = Math.Clamp(state.SimilarComplaintCount * 15 + state.HighSeverityComplaintCount * 10, 0, 100),
            DedupeKey = dedupeKey,
            DetectedAt = now,
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        flag.Events.Add(new MonitoringFlagEvent
        {
            Id = Guid.NewGuid(),
            MonitoringFlagId = flag.Id,
            Type = MonitoringFlagEventType.Created,
            Note = "Raised automatically: repeated product-complaint detection reached HighRisk.",
            ActorUserId = createdByUserId,
            CreatedAt = now,
        });

        await _monitoringRepository.AddFlagAsync(flag, cancellationToken);
        await _monitoringRepository.SaveChangesAsync(cancellationToken);
    }

    private static MonitoringFlagSeverity ToMonitoringSeverity(ReviewSeverity severity) => severity switch
    {
        ReviewSeverity.Critical => MonitoringFlagSeverity.Critical,
        ReviewSeverity.High => MonitoringFlagSeverity.High,
        ReviewSeverity.Medium => MonitoringFlagSeverity.Medium,
        ReviewSeverity.Low => MonitoringFlagSeverity.Low,
        _ => MonitoringFlagSeverity.Info,
    };
}
