// Regression checks for the AI product-review moderation feature (Part 2): repeated/similar-complaint
// detection via RAG retrieval (with a database fallback), a deterministic backend risk state machine
// (Gemini only ever classifies a single comparison), a producer reminder fired exactly once per threshold
// crossing, and an admin alert (existing Governance Monitoring dashboard data) created exactly once at
// HighRisk. No database, no network — in-memory fakes only. Run with `dotnet run`.
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Infrastructure.ReviewModeration;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

ReviewAiAnalysis MakeAnalysis(Guid reviewId, ReviewComplaintType type, ReviewSeverity severity) => new()
{
    Id = Guid.NewGuid(), ReviewId = reviewId, IsNegative = true, IsProductRelated = true,
    ComplaintType = type, Severity = severity, IssueSummary = "Automated test analysis.", Confidence = 0.8,
    IsAiGenerated = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};

var options = Microsoft.Extensions.Options.Options.Create(new ProductModerationOptions
{
    SimilarReviewLookback = 20, WarningThreshold = 3, HighRiskSimilarComplaintThreshold = 5, HighRiskSeverityComplaintThreshold = 10,
});

// ===================== 1 & 2. Rule-based classification: different vs. similar complaints =====================
var ruleProvider = new RuleBasedRepeatedComplaintProvider();

var differentComplaintResult = await ruleProvider.CompareAsync(new RepeatedComplaintContext
{
    ProductName = "Nakshi Kantha",
    NewReviewText = "Delivery was very late.",
    NewComplaintType = ReviewComplaintType.Shipping,
    NewSeverity = ReviewSeverity.Medium,
    HistoricalReviews = new List<HistoricalReviewSnippet>
    {
        new() { ReviewId = Guid.NewGuid(), Text = "Poor quality fabric.", ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High, SameProduct = true },
        new() { ReviewId = Guid.NewGuid(), Text = "The material is too thin.", ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.Medium, SameProduct = true },
    },
}, CancellationToken.None);
Check("unrelated historical complaints are not incorrectly grouped as repeated", !differentComplaintResult.IsRepeatedComplaint);

var similarComplaintResult = await ruleProvider.CompareAsync(new RepeatedComplaintContext
{
    ProductName = "Nakshi Kantha",
    NewReviewText = "The actual quality does not match the description.",
    NewComplaintType = ReviewComplaintType.Quality,
    NewSeverity = ReviewSeverity.High,
    HistoricalReviews = new List<HistoricalReviewSnippet>
    {
        new() { ReviewId = Guid.NewGuid(), Text = "Fabric quality is very poor.", ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High, SameProduct = true },
        new() { ReviewId = Guid.NewGuid(), Text = "The material is too thin.", ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.Medium, SameProduct = true },
    },
}, CancellationToken.None);
Check("semantically similar quality complaints are detected as repeated", similarComplaintResult.IsRepeatedComplaint);
Check("the repeated verdict keeps the shared complaint type", similarComplaintResult.ComplaintType == ReviewComplaintType.Quality);
Check("severity escalates to the worst matching historical severity", similarComplaintResult.Severity == ReviewSeverity.High);

// ===================== Main scenario: a product accumulating repeated complaints =====================
var producerId = Guid.NewGuid();
var productId = Guid.NewGuid();
var product = new Product { Id = productId, Name = "Nakshi Kantha", Slug = "nakshi-kantha", ProducerId = producerId };

Review MakeReview(Guid forProductId, string comment) => new()
{
    Id = Guid.NewGuid(), ProductId = forProductId, UserId = Guid.NewGuid(), Rating = 1,
    Comment = comment, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};

var otherProductId = Guid.NewGuid();
var sameProductHistorical = MakeReview(productId, "Earlier same-product quality complaint.");
var otherProductHistorical = MakeReview(otherProductId, "Complaint about a completely different product.");

var moderationRepo = new FakeProductModerationRepository(product);
moderationRepo.Hydrated[sameProductHistorical.Id] = (sameProductHistorical, MakeAnalysis(sameProductHistorical.Id, ReviewComplaintType.Quality, ReviewSeverity.Low));
moderationRepo.Hydrated[otherProductHistorical.Id] = (otherProductHistorical, MakeAnalysis(otherProductHistorical.Id, ReviewComplaintType.Quality, ReviewSeverity.Low));

var monitoringRepo = new FakeMonitoringRepository();
var similarityProvider = new FakeReviewSimilarityProvider();
var capturingAi = new CapturingRepeatedComplaintAIProvider
{
    Result = new RepeatedComplaintResultDto
    {
        IsRepeatedComplaint = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High,
        Reason = "Multiple customers reported similar material-quality problems.", Confidence = 0.85, IsAiGenerated = true,
    },
};
var service = new RepeatedComplaintDetectionService(
    moderationRepo, monitoringRepo, similarityProvider, capturingAi, options, NullLogger<RepeatedComplaintDetectionService>.Instance);

// ===================== 3. Same ProductId is prioritized in what's shown to the AI =====================
// The RAG result deliberately lists the OTHER product's review first (higher "relevance" score) to prove the
// service itself reorders by same-product, rather than just preserving the retrieval order.
similarityProvider.Result = new List<SimilarReviewMatchDto>
{
    new() { ReviewId = otherProductHistorical.Id, Score = 0.95 },
    new() { ReviewId = sameProductHistorical.Id, Score = 0.40 },
};

var firstReview = MakeReview(productId, "Quality issue number one.");
await service.EvaluateAsync(firstReview, MakeAnalysis(firstReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);

Check("both retrieved candidates were shown to the AI comparison", capturingAi.LastContext!.HistoricalReviews.Count == 2);
Check("the same-product review is prioritized first despite a lower relevance score", capturingAi.LastContext!.HistoricalReviews[0].SameProduct);
Check("the cross-product review is still included, just not first", capturingAi.LastContext!.HistoricalReviews.Any(h => !h.SameProduct));

// From here on, only the same-product historical review is retrieved, to isolate the threshold logic below.
similarityProvider.Result = new List<SimilarReviewMatchDto> { new() { ReviewId = sameProductHistorical.Id, Score = 0.9 } };

// ===================== 4. Repeated complaint count works =====================
Check("the first repeated complaint incremented SimilarComplaintCount", moderationRepo.StateFor(productId).SimilarComplaintCount == 1);
Check("risk state stays Normal below the warning threshold", moderationRepo.StateFor(productId).RiskState == ProductModerationRiskState.Normal);

var secondReview = MakeReview(productId, "Quality issue number two.");
await service.EvaluateAsync(secondReview, MakeAnalysis(secondReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);
Check("a second repeated complaint increments the count again", moderationRepo.StateFor(productId).SimilarComplaintCount == 2);

// ===================== 5 & 6. Warning threshold works; duplicate producer warning is prevented =====================
var thirdReview = MakeReview(productId, "Quality issue number three.");
await service.EvaluateAsync(thirdReview, MakeAnalysis(thirdReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);   // hits WarningThreshold = 3
Check("reaching the warning threshold moves the product to Warning", moderationRepo.StateFor(productId).RiskState == ProductModerationRiskState.Warning);
Check("a producer warning is created at the crossing", moderationRepo.Warnings.Count(w => w.ProductId == productId) == 1);
Check("ProducerWarningCount reflects the single warning issued", moderationRepo.StateFor(productId).ProducerWarningCount == 1);
Check("the notification-carrying warning is addressed to the real producer", moderationRepo.Warnings.Single().ProducerId == producerId);

var fourthReview = MakeReview(productId, "Quality issue number four.");
await service.EvaluateAsync(fourthReview, MakeAnalysis(fourthReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);
Check("staying in the Warning band does not send a second warning for every new review", moderationRepo.Warnings.Count(w => w.ProductId == productId) == 1);

// ===================== 7. High-risk moderation alert is created (existing Admin dashboard data) =====================
var fifthReview = MakeReview(productId, "Quality issue number five.");
await service.EvaluateAsync(fifthReview, MakeAnalysis(fifthReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);   // hits HighRiskSimilarComplaintThreshold = 5
Check("reaching the high-risk threshold moves the product to HighRisk", moderationRepo.StateFor(productId).RiskState == ProductModerationRiskState.HighRisk);

var flag = monitoringRepo.Flags.SingleOrDefault(f => f.SubjectId == productId);
Check("a monitoring flag (the existing Admin dashboard's alert data) is created", flag is not null);
Check("the flag uses the repeated-complaints flag type on the existing entity", flag?.FlagType == MonitoringFlagType.RepeatedProductComplaints);
Check("the flag's evidence identifies the product and producer", flag?.EvidenceJson is { } json1 && json1.Contains(productId.ToString()) && json1.Contains(producerId.ToString()));
Check("the flag's evidence carries the complaint counts", flag?.EvidenceJson is { } json2 && json2.Contains("\"similarComplaintCount\":5"));
Check("the flag's evidence names the triggering review", flag?.EvidenceJson is { } json3 && json3.Contains(fifthReview.Id.ToString()));

var sixthReview = MakeReview(productId, "Quality issue number six.");
await service.EvaluateAsync(sixthReview, MakeAnalysis(sixthReview.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None);
Check("staying in HighRisk does not create a duplicate admin alert", monitoringRepo.Flags.Count(f => f.SubjectId == productId) == 1);

// ===================== 9. RAG failure follows the existing failure-handling pattern (DB fallback) =====================
var productId2 = Guid.NewGuid();
var producerId2 = Guid.NewGuid();
var product2 = new Product { Id = productId2, Name = "Jamdani Saree", Slug = "jamdani-saree", ProducerId = producerId2 };
var fallbackHistorical = MakeReview(productId2, "Historical complaint found only via the database fallback.");

var moderationRepo2 = new FakeProductModerationRepository(product2) { FallbackReviewIds = new List<Guid> { fallbackHistorical.Id } };
moderationRepo2.Hydrated[fallbackHistorical.Id] = (fallbackHistorical, MakeAnalysis(fallbackHistorical.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium));
var ragDownProvider = new FakeReviewSimilarityProvider { Result = null };   // null = RAG unavailable, same contract as IProductSearchCandidateProvider
var capturingAi2 = new CapturingRepeatedComplaintAIProvider
{
    Result = new RepeatedComplaintResultDto { IsRepeatedComplaint = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.Medium, Reason = "db fallback", Confidence = 0.6, IsAiGenerated = false },
};
var serviceWithDownRag = new RepeatedComplaintDetectionService(
    moderationRepo2, monitoringRepo, ragDownProvider, capturingAi2, options, NullLogger<RepeatedComplaintDetectionService>.Instance);

var newReviewDuringOutage = MakeReview(productId2, "New complaint while RAG is down.");
var threwDuringRagOutage = false;
try { await serviceWithDownRag.EvaluateAsync(newReviewDuringOutage, MakeAnalysis(newReviewDuringOutage.Id, ReviewComplaintType.Quality, ReviewSeverity.Medium), CancellationToken.None); }
catch { threwDuringRagOutage = true; }

Check("a RAG outage never throws out of evaluation (degrade, don't break)", !threwDuringRagOutage);
Check("the database fallback still finds the same-product historical review", capturingAi2.CallCount == 1 && capturingAi2.LastContext!.HistoricalReviews.Count == 1);
Check("the database fallback returns only same-product reviews", capturingAi2.LastContext!.HistoricalReviews.All(h => h.SameProduct));
Check("evaluation still records the negative complaint despite the RAG outage", moderationRepo2.StateFor(productId2).NegativeComplaintCount == 1);

// ===================== 8 & 10. Gemini/pipeline failure never invents a result; existing review flow still works =====================
var analysisRepoPositive = new FakeReviewAiAnalysisRepository();
var neverCalledDetection = new CountingRepeatedComplaintService();
var positiveModerationProvider = new FixedReviewModerationProvider(new ReviewModerationResultDto
{
    IsNegative = false, IsProductRelated = true, ComplaintType = ReviewComplaintType.None, Severity = ReviewSeverity.None,
    IssueSummary = "No issue.", Confidence = 0.9, IsAiGenerated = true,
});
var analysisServicePositive = new ReviewAiAnalysisService(analysisRepoPositive, positiveModerationProvider, neverCalledDetection, NullLogger<ReviewAiAnalysisService>.Instance);
var positiveReview = MakeReview(productId, "Lovely product, thank you!");
await analysisServicePositive.AnalyzeReviewAsync(positiveReview, product, CancellationToken.None);
Check("existing review functionality: a normal review never triggers repeated-complaint detection", neverCalledDetection.CallCount == 0);
Check("existing review functionality: the normal review's own analysis is still stored", analysisRepoPositive.CountForReview(positiveReview.Id) == 1);

var analysisRepoNegative = new FakeReviewAiAnalysisRepository();
var throwingDetection = new ThrowingRepeatedComplaintService();
var negativeModerationProvider = new FixedReviewModerationProvider(new ReviewModerationResultDto
{
    IsNegative = true, IsProductRelated = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High,
    IssueSummary = "Bad quality.", Confidence = 0.9, IsAiGenerated = true,
});
var analysisServiceNegative = new ReviewAiAnalysisService(analysisRepoNegative, negativeModerationProvider, throwingDetection, NullLogger<ReviewAiAnalysisService>.Instance);
var negativeReview = MakeReview(productId, "Terrible quality, fell apart.");
var threwFromPipeline = false;
try { await analysisServiceNegative.AnalyzeReviewAsync(negativeReview, product, CancellationToken.None); }
catch { threwFromPipeline = true; }
Check("a total repeated-complaint pipeline failure never invents a result nor breaks review analysis", !threwFromPipeline);
Check("the review's own AI analysis is still stored despite the downstream failure", analysisRepoNegative.CountForReview(negativeReview.Id) == 1);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

// ---- fakes ---------------------------------------------------------------------------------------------------

class CapturingRepeatedComplaintAIProvider : IRepeatedComplaintAIProvider
{
    public RepeatedComplaintResultDto Result { get; set; } = new();
    public RepeatedComplaintContext? LastContext { get; private set; }
    public int CallCount { get; private set; }

    public Task<RepeatedComplaintResultDto> CompareAsync(RepeatedComplaintContext context, CancellationToken cancellationToken)
    {
        CallCount++;
        LastContext = context;
        return Task.FromResult(Result);
    }
}

class FakeReviewSimilarityProvider : IReviewSimilarityProvider
{
    public List<SimilarReviewMatchDto>? Result { get; set; } = new();

    public Task<List<SimilarReviewMatchDto>?> FindSimilarAsync(Guid productId, string queryText, int limit, CancellationToken cancellationToken)
        => Task.FromResult(Result);
}

class CountingRepeatedComplaintService : IRepeatedComplaintDetectionService
{
    public int CallCount { get; private set; }
    public Task EvaluateAsync(Review review, ReviewAiAnalysis analysis, CancellationToken cancellationToken) { CallCount++; return Task.CompletedTask; }
}

class ThrowingRepeatedComplaintService : IRepeatedComplaintDetectionService
{
    public Task EvaluateAsync(Review review, ReviewAiAnalysis analysis, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Simulated repeated-complaint pipeline failure.");
}

class FixedReviewModerationProvider : IReviewModerationAIProvider
{
    private readonly ReviewModerationResultDto _result;
    public FixedReviewModerationProvider(ReviewModerationResultDto result) => _result = result;
    public Task<ReviewModerationResultDto> AnalyzeAsync(ReviewModerationContext context, CancellationToken cancellationToken) => Task.FromResult(_result);
}

class FakeReviewAiAnalysisRepository : IReviewAiAnalysisRepository
{
    private readonly Dictionary<Guid, ReviewAiAnalysis> _byReview = new();

    public int CountForReview(Guid reviewId) => _byReview.ContainsKey(reviewId) ? 1 : 0;

    public Task<bool> ExistsForReviewAsync(Guid reviewId, CancellationToken cancellationToken) => Task.FromResult(_byReview.ContainsKey(reviewId));
    public Task<ReviewAiAnalysis?> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken) => Task.FromResult(_byReview.GetValueOrDefault(reviewId));

    public Task AddAsync(ReviewAiAnalysis analysis, CancellationToken cancellationToken)
    {
        if (!_byReview.TryAdd(analysis.ReviewId, analysis))
        {
            throw new InvalidOperationException($"Duplicate analysis for review {analysis.ReviewId}.");
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

class FakeProductModerationRepository : IProductModerationRepository
{
    private readonly Dictionary<Guid, ProductModerationState> _states = new();
    private readonly Product _product;

    public List<ProductModerationEvent> Events { get; } = new();
    public List<ProducerModerationWarning> Warnings { get; } = new();
    public List<Guid> FallbackReviewIds { get; set; } = new();
    public Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)> Hydrated { get; } = new();

    public FakeProductModerationRepository(Product product) => _product = product;

    public ProductModerationState StateFor(Guid productId) => _states[productId];

    public Task<ProductModerationState> GetOrCreateStateAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (!_states.TryGetValue(productId, out var state))
        {
            state = new ProductModerationState { ProductId = productId, Product = _product, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _states[productId] = state;
        }

        return Task.FromResult(state);
    }

    public Task<List<Guid>> GetRecentNegativeReviewIdsForProductAsync(Guid productId, Guid excludeReviewId, int limit, CancellationToken cancellationToken)
        => Task.FromResult(FallbackReviewIds.Where(id => id != excludeReviewId).Take(limit).ToList());

    public Task<Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)>> GetReviewsWithAnalysisByIdsAsync(
        IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => Task.FromResult(Hydrated.Where(kv => reviewIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public Task<List<ProductModerationEvent>> GetRecentEventsAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => Task.FromResult(Events.Where(e => e.ProductId == productId).OrderByDescending(e => e.CreatedAt).Take(limit).ToList());

    public Task<int> CountNegativeReviewsForProductAsync(Guid productId, CancellationToken cancellationToken) => Task.FromResult(0);

    public Task<List<ProducerModerationWarning>> GetWarningsForProductAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => Task.FromResult(Warnings.Where(w => w.ProductId == productId).OrderByDescending(w => w.CreatedAt).Take(limit).ToList());

    public Task<(List<ProductModerationCaseRow> Items, int TotalCount)> GetCasesAsync(
        ProductModerationRiskState? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken)
        => Task.FromResult((new List<ProductModerationCaseRow>(), 0));

    public Task AddEventAsync(ProductModerationEvent moderationEvent, CancellationToken cancellationToken) { Events.Add(moderationEvent); return Task.CompletedTask; }
    public Task AddWarningAsync(ProducerModerationWarning warning, CancellationToken cancellationToken) { Warnings.Add(warning); return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

class FakeMonitoringRepository : IMonitoringRepository
{
    public List<MonitoringFlag> Flags { get; } = new();
    public Guid? SuperAdminId { get; set; } = Guid.NewGuid();

    public Task AddFlagAsync(MonitoringFlag flag, CancellationToken cancellationToken) { Flags.Add(flag); return Task.CompletedTask; }
    public void RemoveFlag(MonitoringFlag flag) => Flags.Remove(flag);
    public Task<MonitoringFlag?> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Flags.FirstOrDefault(f => f.Id == id));
    public Task<(List<MonitoringFlag> Items, int TotalCount)> GetFlagsPagedAsync(MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
        => Task.FromResult((new List<MonitoringFlag>(), 0));

    public Task<HashSet<string>> GetOpenFlagDedupeKeysAsync(IEnumerable<string> candidateKeys, CancellationToken cancellationToken)
    {
        var keys = candidateKeys.ToHashSet();
        var open = Flags.Where(f => keys.Contains(f.DedupeKey) && f.Status != MonitoringFlagStatus.Dismissed && f.Status != MonitoringFlagStatus.Resolved)
            .Select(f => f.DedupeKey).ToHashSet();
        return Task.FromResult(open);
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<Guid?> GetAnySuperAdminUserIdAsync(CancellationToken cancellationToken) => Task.FromResult(SuperAdminId);

    public Task<List<ScanCandidate>> FindFraudCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindFakeProductCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindReviewAbuseCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindQrAnomalyCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindSpamContentCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindPolicyViolationCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<List<ScanCandidate>> FindInappropriateImageCandidatesAsync(DateTime since, CancellationToken cancellationToken) => Task.FromResult(new List<ScanCandidate>());
    public Task<QrMonitoringOverviewDto> GetQrOverviewAsync(DateTime? from, DateTime? to, int topN, CancellationToken cancellationToken) => Task.FromResult(new QrMonitoringOverviewDto());

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
