// Regression checks for the AI product-review moderation feature (Part 1): a normal review produces no
// complaint, a negative review is classified correctly, analysis results are stored exactly once per review,
// a Gemini failure never invents a result nor breaks anything, and the review index feed carries the right
// ReviewId/ProductId/ProducerId/ComplaintType/Severity metadata without ever creating duplicate index rows.
// No database, no network — in-memory fakes only. Run with `dotnet run`.
using Microsoft.Extensions.Logging.Abstractions;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Infrastructure.ReviewModeration;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

var producerId = Guid.NewGuid();
var productId = Guid.NewGuid();
var product = new Product { Id = productId, Name = "Nakshi Kantha", Slug = "nakshi-kantha", ProducerId = producerId };

Review MakeReview(string comment, int rating) => new()
{
    Id = Guid.NewGuid(), ProductId = productId, Product = product, UserId = Guid.NewGuid(),
    Rating = rating, Comment = comment, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};

// ===================== 1. Normal (positive) review: rule-based provider =====================
var ruleProvider = new RuleBasedReviewModerationProvider();
var positiveResult = await ruleProvider.AnalyzeAsync(
    new ReviewModerationContext { Comment = "Beautiful craftsmanship, exactly as described. Highly recommend!", Rating = 5, ProductName = product.Name },
    CancellationToken.None);
Check("a normal 5-star review is not flagged as negative", !positiveResult.IsNegative);
Check("a non-negative review carries no complaint type", positiveResult.ComplaintType == ReviewComplaintType.None);
Check("a non-negative review carries no severity", positiveResult.Severity == ReviewSeverity.None);
Check("the rule-based provider marks its output as not AI-generated", !positiveResult.IsAiGenerated);

// ===================== 2. Negative review: rule-based provider =====================
var negativeResult = await ruleProvider.AnalyzeAsync(
    new ReviewModerationContext { Comment = "The product arrived broken and the quality is very poor.", Rating = 1, ProductName = product.Name },
    CancellationToken.None);
Check("a 1-star review with quality complaints is flagged as negative", negativeResult.IsNegative);
Check("quality-related keywords classify the complaint as Quality", negativeResult.ComplaintType == ReviewComplaintType.Quality);
Check("a quality complaint is treated as product-related", negativeResult.IsProductRelated);
Check("a 1-star review gets a high severity", negativeResult.Severity is ReviewSeverity.High or ReviewSeverity.Critical);
Check("confidence is reported within the valid 0-1 range", negativeResult.Confidence is >= 0 and <= 1);

// A shipping-only complaint should not be attributed to the product itself.
var shippingResult = await ruleProvider.AnalyzeAsync(
    new ReviewModerationContext { Comment = "Delivery was very late and the courier was unhelpful.", Rating = 2, ProductName = product.Name },
    CancellationToken.None);
Check("a delivery-only complaint is classified as Shipping", shippingResult.ComplaintType == ReviewComplaintType.Shipping);
Check("a shipping complaint is not attributed to the product itself", !shippingResult.IsProductRelated);

// ===================== 3 & 4. AI analysis is run once per review and stored correctly =====================
var analysisRepo = new FakeReviewAiAnalysisRepository();
var capturingProvider = new CapturingReviewModerationProvider(new ReviewModerationResultDto
{
    IsNegative = true, IsProductRelated = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High,
    IssueSummary = "Customer reports poor product quality.", Confidence = 0.91, IsAiGenerated = true,
});
var noopRepeatedComplaintService = new NoopRepeatedComplaintService();
var analysisService = new ReviewAiAnalysisService(analysisRepo, capturingProvider, noopRepeatedComplaintService, NullLogger<ReviewAiAnalysisService>.Instance);

var review = MakeReview("Terrible quality, fell apart after one use.", 1);
await analysisService.AnalyzeReviewAsync(review, product, CancellationToken.None);

Check("the AI provider was called exactly once for a new review", capturingProvider.CallCount == 1);
Check("the provider received the review's own comment and rating (grounding)",
    capturingProvider.LastContext!.Comment == review.Comment && capturingProvider.LastContext.Rating == review.Rating
    && capturingProvider.LastContext.ProductName == product.Name);

var stored = await analysisRepo.GetByReviewIdAsync(review.Id, CancellationToken.None);
Check("an AI analysis row was stored for the review", stored is not null);
Check("the stored analysis matches the provider's verdict exactly",
    stored!.IsNegative && stored.IsProductRelated && stored.ComplaintType == ReviewComplaintType.Quality
    && stored.Severity == ReviewSeverity.High && stored.Confidence == 0.91 && stored.IsAiGenerated);

// Re-running analysis for the same review must not call the provider again or create a second row.
await analysisService.AnalyzeReviewAsync(review, product, CancellationToken.None);
Check("re-analyzing the same review does not call the AI provider again", capturingProvider.CallCount == 1);
Check("re-analyzing the same review does not create a duplicate analysis row", analysisRepo.CountForReview(review.Id) == 1);

// ===================== 8. Gemini/provider failure: no result is invented, nothing else breaks =====================
var throwingProvider = new ThrowingReviewModerationProvider();
var failureAnalysisRepo = new FakeReviewAiAnalysisRepository();
var failureService = new ReviewAiAnalysisService(failureAnalysisRepo, throwingProvider, noopRepeatedComplaintService, NullLogger<ReviewAiAnalysisService>.Instance);
var reviewForFailure = MakeReview("This is fine I guess.", 3);

var threw = false;
try { await failureService.AnalyzeReviewAsync(reviewForFailure, product, CancellationToken.None); }
catch { threw = true; }
Check("an unexpected AI-provider failure never propagates out of the analysis service", !threw);
Check("no analysis row is invented when the AI provider fails", failureAnalysisRepo.CountForReview(reviewForFailure.Id) == 0);

// ===================== 5, 6, 7. Review vector/indexing: correct ProductId/ProducerId metadata =====================
var indexRepo = new FakeReviewIndexRepository();
indexRepo.SeedReview(review);
indexRepo.SeedAnalysis(stored!);
indexRepo.MarkDirty(review.Id);

var indexService = new ReviewIndexService(indexRepo);
var batch = await indexService.GetPendingAsync(50, CancellationToken.None);

Check("a newly dirty review appears in the pending batch", batch.Items.Any(i => i.ReviewId == review.Id));
var item = batch.Items.Single(i => i.ReviewId == review.Id);
Check("a never-synced review is queued as an upsert (needs embedding)", item.Action == "upsert");
Check("the indexed text includes the review's own comment", item.Text.Contains("Terrible quality"));
Check("payload carries the correct ReviewId", (Guid)item.Payload["review_id"]! == review.Id);
Check("payload carries the correct ProductId", (Guid?)item.Payload["product_id"] == productId);
Check("payload carries the correct ProducerId", (Guid?)item.Payload["producer_id"] == producerId);
Check("payload carries the AI-derived ComplaintType", (string?)item.Payload["complaint_type"] == "Quality");
Check("payload carries the AI-derived Severity", (string?)item.Payload["severity"] == "High");
Check("payload carries the review's CreatedAt", (DateTime)item.Payload["created_at"]! == review.CreatedAt);

// ===================== 9. Duplicate vector/index prevention =====================
// The review was already marked dirty once above (before the pending-batch checks); mark it dirty again here
// to simulate a second change and confirm this never creates a second index-state row for the same review.
var versionBeforeSecondMark = indexRepo.GetVersion(review.Id);
indexRepo.MarkDirty(review.Id);
Check("marking the same review dirty twice keeps a single index-state row (no duplicate)", indexRepo.StateCount == 1);
Check("a repeated dirty-mark bumps the version instead of creating a new row", indexRepo.GetVersion(review.Id) == versionBeforeSecondMark + 1);

// Acking clears Dirty; the same review must not reappear in the next pending batch until it changes again.
await indexService.AckAsync(new ReviewIndexAckRequest
{
    Items = new List<ReviewIndexAckItem> { new() { ReviewId = review.Id, Version = indexRepo.GetVersion(review.Id), Success = true, TextHash = item.TextHash } },
}, CancellationToken.None);
var batchAfterAck = await indexService.GetPendingAsync(50, CancellationToken.None);
Check("after a successful ack the review is no longer pending (no re-indexing loop)", batchAfterAck.Items.All(i => i.ReviewId != review.Id));
Check("acking never creates a second state row for the same review", indexRepo.StateCount == 1);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class NoopRepeatedComplaintService : IRepeatedComplaintDetectionService
{
    public Task EvaluateAsync(Review review, ReviewAiAnalysis analysis, CancellationToken cancellationToken) => Task.CompletedTask;
}

class CapturingReviewModerationProvider : IReviewModerationAIProvider
{
    private readonly ReviewModerationResultDto _result;
    public int CallCount { get; private set; }
    public ReviewModerationContext? LastContext { get; private set; }

    public CapturingReviewModerationProvider(ReviewModerationResultDto result) => _result = result;

    public Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken)
    {
        CallCount++;
        LastContext = context;
        return Task.FromResult(_result);
    }
}

class ThrowingReviewModerationProvider : IReviewModerationAIProvider
{
    public Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Simulated Gemini/provider outage.");
}

class FakeReviewAiAnalysisRepository : IReviewAiAnalysisRepository
{
    private readonly Dictionary<Guid, ReviewAiAnalysis> _byReview = new();

    public int CountForReview(Guid reviewId) => _byReview.ContainsKey(reviewId) ? 1 : 0;

    public Task<bool> ExistsForReviewAsync(Guid reviewId, CancellationToken cancellationToken) => Task.FromResult(_byReview.ContainsKey(reviewId));
    public Task<ReviewAiAnalysis?> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken) => Task.FromResult(_byReview.GetValueOrDefault(reviewId));

    public Task AddAsync(ReviewAiAnalysis analysis, CancellationToken cancellationToken)
    {
        // Mirrors the DB's unique index on ReviewId: a second insert for the same review is a bug, not a no-op.
        if (!_byReview.TryAdd(analysis.ReviewId, analysis))
        {
            throw new InvalidOperationException($"Duplicate analysis for review {analysis.ReviewId}.");
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

class FakeReviewIndexRepository : IReviewIndexRepository
{
    private readonly Dictionary<Guid, ReviewIndexState> _states = new();
    private readonly Dictionary<Guid, Review> _reviews = new();
    private readonly Dictionary<Guid, ReviewAiAnalysis> _analyses = new();

    public int StateCount => _states.Count;
    public int GetVersion(Guid reviewId) => _states[reviewId].Version;

    public void SeedReview(Review review) => _reviews[review.Id] = review;
    public void SeedAnalysis(ReviewAiAnalysis analysis) => _analyses[analysis.ReviewId] = analysis;

    /// <summary>Simulates what ReviewIndexDirtyInterceptor does on save: upsert-by-ReviewId, bump version.</summary>
    public void MarkDirty(Guid reviewId)
    {
        if (!_states.TryGetValue(reviewId, out var state))
        {
            state = new ReviewIndexState { ReviewId = reviewId };
            _states[reviewId] = state;
        }

        state.Status = IndexStatuses.Dirty;
        state.Version++;
        state.AttemptCount = 0;
        state.LastError = null;
        state.UpdatedAt = DateTime.UtcNow;
    }

    private IEnumerable<ReviewIndexState> Pending(int maxAttempts)
        => _states.Values.Where(s => s.Status == IndexStatuses.Dirty || s.Status == IndexStatuses.Deleted
            || (s.Status == IndexStatuses.Failed && s.AttemptCount < maxAttempts));

    public Task<List<ReviewIndexState>> GetPendingAsync(int limit, int maxAttempts, CancellationToken cancellationToken)
        => Task.FromResult(Pending(maxAttempts).OrderBy(s => s.UpdatedAt).Take(limit).ToList());

    public Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken) => Task.FromResult(Pending(maxAttempts).Count());

    public Task<Dictionary<Guid, ReviewIndexState>> GetStatesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => Task.FromResult(_states.Where(kv => reviewIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public Task<Dictionary<Guid, Review>> GetReviewsForIndexAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => Task.FromResult(_reviews.Where(kv => reviewIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public Task<Dictionary<Guid, ReviewAiAnalysis>> GetAnalysesAsync(IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => Task.FromResult(_analyses.Where(kv => reviewIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public void RemoveState(ReviewIndexState state) => _states.Remove(state.ReviewId);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
