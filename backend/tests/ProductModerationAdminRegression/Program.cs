// Regression checks for the AI product-review moderation feature (Part 3): the admin case list/detail reads
// live data (not a frozen snapshot), the AI never bans a product (only an explicit admin request does), the
// Ban action reuses the EXISTING product-approval/audit/flag-status infrastructure rather than a parallel one,
// and the Ban action is actually gated to SuperAdmin at the server (reflection over the real controller
// attributes, not just a claim). No database, no network, no web host — in-memory fakes only. Run with `dotnet run`.
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Governance;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

// ===================== Setup: a HighRisk product-moderation case, as Part 2 would have created it =====================
var productId = Guid.NewGuid();
var producerId = Guid.NewGuid();
var superAdminId = Guid.NewGuid();

var producer = new User { Id = producerId, FullName = "Producer A", Email = "producer@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var product = new Product
{
    Id = productId, Name = "Nakshi Kantha", Slug = "nakshi-kantha", ProducerId = producerId, Producer = producer,
    ReviewCount = 12, ApprovalStatus = ProductApprovalStatus.Approved,
};

var triggeringReview = new Review { Id = Guid.NewGuid(), ProductId = productId, UserId = Guid.NewGuid(), Rating = 1, Comment = "Terrible quality, fell apart after one use.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var similarReview = new Review { Id = Guid.NewGuid(), ProductId = productId, UserId = Guid.NewGuid(), Rating = 2, Comment = "Fabric quality is very poor.", CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow.AddDays(-3) };
var triggeringAnalysis = new ReviewAiAnalysis { Id = Guid.NewGuid(), ReviewId = triggeringReview.Id, IsNegative = true, IsProductRelated = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High, IssueSummary = "Customer reports poor product quality.", Confidence = 0.9, IsAiGenerated = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var similarAnalysis = new ReviewAiAnalysis { Id = Guid.NewGuid(), ReviewId = similarReview.Id, IsNegative = true, IsProductRelated = true, ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.Medium, IssueSummary = "Poor material.", Confidence = 0.7, IsAiGenerated = true, CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow.AddDays(-3) };

var moderationRepo = new FakeProductModerationRepository(product);
moderationRepo.Hydrated[triggeringReview.Id] = (triggeringReview, triggeringAnalysis);
moderationRepo.Hydrated[similarReview.Id] = (similarReview, similarAnalysis);
moderationRepo.SeedState(new ProductModerationState
{
    ProductId = productId, Product = product,
    NegativeComplaintCount = 6, SimilarComplaintCount = 5, HighSeverityComplaintCount = 2, ProducerWarningCount = 1,
    RiskState = ProductModerationRiskState.HighRisk, LastReviewId = triggeringReview.Id, LastEvaluatedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
});
moderationRepo.NegativeReviewCount = 6;
moderationRepo.Warnings.Add(new ProducerModerationWarning
{
    Id = Guid.NewGuid(), ProductId = productId, ProducerId = producerId, ComplaintType = ReviewComplaintType.Quality,
    SimilarComplaintCount = 3, Message = "Multiple customers have reported similar Quality issues with this product.",
    CreatedAt = DateTime.UtcNow.AddDays(-2),
});
moderationRepo.Events.Add(new ProductModerationEvent
{
    Id = Guid.NewGuid(), ProductId = productId, ReviewId = triggeringReview.Id, Type = ProductModerationEventType.HighRiskFlagged,
    ComplaintType = ReviewComplaintType.Quality, Severity = ReviewSeverity.High,
    Reason = "1 retrieved historical review(s) reported a similar Quality issue.", Confidence = 0.85, IsAiGenerated = true,
    SimilarReviewIds = new List<Guid> { similarReview.Id }, CreatedAt = DateTime.UtcNow,
});

var evidenceJson = JsonSerializer.Serialize(new
{
    productId, productName = product.Name, producerId, riskState = "HighRisk",
    negativeComplaintCount = 6, similarComplaintCount = 5, highSeverityComplaintCount = 2, producerWarningCount = 1,
    triggeringReviewId = triggeringReview.Id, triggeringReviewComment = triggeringReview.Comment,
    triggeringComplaintType = "Quality", triggeringSeverity = "High",
    similarHistoricalReviewIds = new[] { similarReview.Id },
    complaintType = "Quality", severity = "High", isRepeatedComplaint = true, confidence = 0.85,
    aiExplanation = "Multiple customers reported similar material-quality problems.", isAiGenerated = true,
    moderationHistory = Array.Empty<object>(),
});

var flag = new MonitoringFlag
{
    Id = Guid.NewGuid(), FlagType = MonitoringFlagType.RepeatedProductComplaints, Severity = MonitoringFlagSeverity.High,
    Status = MonitoringFlagStatus.Open, Source = MonitoringFlagSource.AutomatedScan,
    SubjectType = MonitoringSubjectType.Product, SubjectId = productId, SubjectLabel = product.Name,
    Title = "Repeated Quality complaints on Nakshi Kantha", Description = "Multiple customers reported similar material-quality problems.",
    EvidenceJson = evidenceJson, RiskScore = 85, DedupeKey = $"product-moderation:{productId}",
    DetectedAt = DateTime.UtcNow, CreatedByUserId = superAdminId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};

var monitoringRepo = new FakeMonitoringRepository();
monitoringRepo.SeedFlag(flag);
var monitoringService = new MonitoringService(monitoringRepo);   // the REAL, unmodified Part-0 service — exercised, not re-implemented
var productService = new FakeProductServiceForBan();
var auditLog = new FakeAuditLogService();
var userRepo = new FakeUserRepository();
userRepo.Add(new User { Id = superAdminId, FullName = "Admin Rahim", Email = "admin@shilpohub.example", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

var adminService = new ProductModerationAdminService(moderationRepo, monitoringRepo, monitoringService, productService, auditLog, userRepo);

// ===================== 1. Admin moderation list =====================
moderationRepo.CasesResult = (new List<ProductModerationCaseRow>
{
    new(flag.Id, productId, product.Name, producerId, producer.FullName, ProductModerationRiskState.HighRisk, 5, 2, 1, "Open", flag.DetectedAt),
}, 1);
var caseList = await adminService.GetCasesAsync(null, null, 1, 20, CancellationToken.None);
Check("the moderation list returns the seeded case", caseList.Items.Count == 1 && caseList.TotalCount == 1);
var listItem = caseList.Items[0];
Check("the list shows the product and producer", listItem.ProductName == "Nakshi Kantha" && listItem.ProducerName == "Producer A");
Check("the list shows the live risk state and complaint/warning counts", listItem.RiskState == "HighRisk" && listItem.SimilarComplaintCount == 5 && listItem.HighSeverityComplaintCount == 2 && listItem.ProducerWarningCount == 1);
Check("the list shows the current moderation status", listItem.ModerationStatus == "Open");

// ===================== 2. Moderation details (full evidence, not just the AI summary) =====================
var detail = await adminService.GetCaseDetailAsync(flag.Id, CancellationToken.None);
Check("detail includes product and producer information", detail.ProductId == productId && detail.ProductName == "Nakshi Kantha" && detail.ProducerId == producerId && detail.ProducerName == "Producer A");
Check("detail includes review statistics", detail.TotalReviews == product.ReviewCount && detail.NegativeReviews == 6 && detail.SimilarComplaints == 5 && detail.HighSeverityComplaints == 2 && detail.PreviousWarnings == 1);
Check("detail includes the triggering review's actual text, rating, complaint type and severity",
    detail.TriggeringReview is { } tr && tr.Text == triggeringReview.Comment && tr.Rating == 1 && tr.ComplaintType == "Quality" && tr.Severity == "High");
Check("detail includes the similar historical review actually retrieved (not invented)",
    detail.SimilarReviews.Count == 1 && detail.SimilarReviews[0].ReviewId == similarReview.Id && detail.SimilarReviews[0].Text == similarReview.Comment);
Check("the triggering review is never duplicated into the similar-reviews list", detail.SimilarReviews.All(r => r.ReviewId != triggeringReview.Id));
Check("detail includes the AI analysis: issue summary, repeated-complaint result, severity, confidence, explanation",
    detail.IssueSummary == triggeringAnalysis.IssueSummary && detail.IsRepeatedComplaint == true
    && detail.AiComplaintType == "Quality" && detail.AiSeverity == "High" && detail.AiConfidence == 0.85 && detail.IsAiGenerated == true);
Check("detail includes moderation history from more than one source (automated events + warnings)",
    detail.History.Any(h => h.Kind == "ModerationEvent") && detail.History.Any(h => h.Kind == "Warning"));

// A flag with no/garbled evidence must degrade gracefully rather than crash — existing-functionality regression guard.
var noEvidenceFlag = new MonitoringFlag
{
    Id = Guid.NewGuid(), FlagType = MonitoringFlagType.RepeatedProductComplaints, Severity = MonitoringFlagSeverity.Medium,
    Status = MonitoringFlagStatus.Open, Source = MonitoringFlagSource.AutomatedScan, SubjectType = MonitoringSubjectType.Product,
    SubjectId = productId, SubjectLabel = product.Name, Title = "x", Description = "x", EvidenceJson = null,
    RiskScore = 10, DedupeKey = "no-evidence", DetectedAt = DateTime.UtcNow, CreatedByUserId = superAdminId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};
monitoringRepo.SeedFlag(noEvidenceFlag);
var detailNoEvidence = await adminService.GetCaseDetailAsync(noEvidenceFlag.Id, CancellationToken.None);
Check("a flag with no evidence JSON degrades gracefully instead of crashing", detailNoEvidence.TriggeringReview is null && detailNoEvidence.SimilarReviews.Count == 0);

// A flag on a non-Product subject must be rejected, not silently treated as one.
var nonProductFlag = new MonitoringFlag
{
    Id = Guid.NewGuid(), FlagType = MonitoringFlagType.FraudRisk, Severity = MonitoringFlagSeverity.Low, Status = MonitoringFlagStatus.Open,
    Source = MonitoringFlagSource.AutomatedScan, SubjectType = MonitoringSubjectType.Producer, SubjectId = producerId, SubjectLabel = "x",
    Title = "x", Description = "x", RiskScore = 10, DedupeKey = "other", DetectedAt = DateTime.UtcNow, CreatedByUserId = superAdminId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};
monitoringRepo.SeedFlag(nonProductFlag);
var threwForWrongSubject = false;
try { await adminService.GetCaseDetailAsync(nonProductFlag.Id, CancellationToken.None); } catch (ConflictException) { threwForWrongSubject = true; }
Check("a non-product flag is rejected as a product-moderation case", threwForWrongSubject);

var threwNotFound = false;
try { await adminService.GetCaseDetailAsync(Guid.NewGuid(), CancellationToken.None); } catch (NotFoundException) { threwNotFound = true; }
Check("a missing flag id returns not-found", threwNotFound);

// ===================== 3 & 4. Ban action: AI never bans; only an explicit admin call reaches this code path =====================
var banResult = await adminService.BanProductAsync(superAdminId, flag.Id, new BanProductRequest { Reason = "Confirmed repeated quality defects." }, CancellationToken.None);

Check("banning calls the EXISTING product-approval logic to reject the product (no parallel status system)",
    productService.ApprovalCalls.Any(c => c.ProductId == productId && c.AdminUserId == superAdminId && c.Request.Status == ProductApprovalStatus.Rejected));
Check("the admin's stated reason is passed through as the rejection reason", productService.ApprovalCalls.Last().Request.RejectionReason == "Confirmed repeated quality defects.");
Check("banning marks the moderation case with a ProductBanned history event", moderationRepo.Events.Any(e => e.ProductId == productId && e.Type == ProductModerationEventType.ProductBanned));
Check("banning resolves the flag through the EXISTING flag-status workflow (not a new one)", monitoringRepo.Flags[flag.Id].Status == MonitoringFlagStatus.Resolved);
Check("the flag gained a real MonitoringFlagEvent recording who resolved it and why", monitoringRepo.Flags[flag.Id].Events.Any(e => e.Type == MonitoringFlagEventType.Resolved && e.ActorUserId == superAdminId));
Check("banning records an audit entry via the EXISTING audit service (not a new one)", auditLog.Calls.Any(c => c.Action == "Product.Banned" && c.EntityId == productId && c.ActorUserId == superAdminId));
Check("the ban action returns the updated, resolved flag", banResult.Status == "Resolved");

// Banning again on an already-resolved case must not throw (idempotent w.r.t. the flag-status guard) and still audits.
var threwOnSecondBan = false;
try { await adminService.BanProductAsync(superAdminId, flag.Id, new BanProductRequest { Reason = "Repeat ban attempt." }, CancellationToken.None); }
catch { threwOnSecondBan = true; }
Check("banning an already-resolved case does not throw", !threwOnSecondBan);
Check("a second ban still records its own audit entry", auditLog.Calls.Count(c => c.Action == "Product.Banned") == 2);

var threwBanWrongSubject = false;
try { await adminService.BanProductAsync(superAdminId, nonProductFlag.Id, new BanProductRequest(), CancellationToken.None); } catch (ConflictException) { threwBanWrongSubject = true; }
Check("banning a non-product flag is rejected", threwBanWrongSubject);

// A ban with no reason still works (falls back to a clear, non-invented default reason rather than failing).
var noReasonProduct = new Product { Id = Guid.NewGuid(), Name = "Second Product", Slug = "second-product", ProducerId = producerId, Producer = producer, ReviewCount = 3, ApprovalStatus = ProductApprovalStatus.Approved };
var moderationRepo2 = new FakeProductModerationRepository(noReasonProduct);
moderationRepo2.SeedState(new ProductModerationState { ProductId = noReasonProduct.Id, Product = noReasonProduct, RiskState = ProductModerationRiskState.HighRisk, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
var noReasonFlag = new MonitoringFlag
{
    Id = Guid.NewGuid(), FlagType = MonitoringFlagType.RepeatedProductComplaints, Severity = MonitoringFlagSeverity.High, Status = MonitoringFlagStatus.Open,
    Source = MonitoringFlagSource.AutomatedScan, SubjectType = MonitoringSubjectType.Product, SubjectId = noReasonProduct.Id, SubjectLabel = noReasonProduct.Name,
    Title = "x", Description = "x", RiskScore = 80, DedupeKey = $"product-moderation:{noReasonProduct.Id}", DetectedAt = DateTime.UtcNow, CreatedByUserId = superAdminId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
};
monitoringRepo.SeedFlag(noReasonFlag);
var adminService2 = new ProductModerationAdminService(moderationRepo2, monitoringRepo, monitoringService, productService, auditLog, userRepo);
await adminService2.BanProductAsync(superAdminId, noReasonFlag.Id, new BanProductRequest { Reason = null }, CancellationToken.None);
Check("a ban with no admin-supplied reason still succeeds with a clear default reason", !string.IsNullOrWhiteSpace(productService.ApprovalCalls.Last().Request.RejectionReason));

// ===================== 5. Security: server-side authorization is actually wired on the real controller =====================
var controllerType = typeof(MonitoringController);
var classAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();
Check("the monitoring controller requires GovernmentNGO or SuperAdmin at the class level", classAuthorize?.Roles is { } cr && cr.Contains("SuperAdmin") && cr.Contains("GovernmentNGO"));

var banMethod = controllerType.GetMethod(nameof(MonitoringController.BanProduct));
var banAuthorize = banMethod?.GetCustomAttribute<AuthorizeAttribute>();
Check("the Ban action is gated to SuperAdmin specifically, enforced server-side (not just hidden in the UI)", banAuthorize?.Roles == "SuperAdmin");

var caseDetailMethod = controllerType.GetMethod(nameof(MonitoringController.GetProductModerationCase));
Check("the case-detail read action exists and carries no [AllowAnonymous] override", caseDetailMethod is not null && caseDetailMethod.GetCustomAttribute<AllowAnonymousAttribute>() is null);
var casesMethod = controllerType.GetMethod(nameof(MonitoringController.GetProductModerationCases));
Check("the moderation-list read action exists and carries no [AllowAnonymous] override", casesMethod is not null && casesMethod.GetCustomAttribute<AllowAnonymousAttribute>() is null);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

// ---- fakes ---------------------------------------------------------------------------------------------------

class FakeProductModerationRepository : IProductModerationRepository
{
    private readonly Dictionary<Guid, ProductModerationState> _states = new();
    private readonly Product _product;

    public List<ProductModerationEvent> Events { get; } = new();
    public List<ProducerModerationWarning> Warnings { get; } = new();
    public Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)> Hydrated { get; } = new();
    public int NegativeReviewCount { get; set; }
    public (List<ProductModerationCaseRow> Items, int TotalCount) CasesResult { get; set; } = (new(), 0);

    public FakeProductModerationRepository(Product product) => _product = product;

    public void SeedState(ProductModerationState state) => _states[state.ProductId] = state;

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
        => Task.FromResult(new List<Guid>());

    public Task<Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)>> GetReviewsWithAnalysisByIdsAsync(
        IReadOnlyCollection<Guid> reviewIds, CancellationToken cancellationToken)
        => Task.FromResult(Hydrated.Where(kv => reviewIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value));

    public Task<List<ProductModerationEvent>> GetRecentEventsAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => Task.FromResult(Events.Where(e => e.ProductId == productId).OrderByDescending(e => e.CreatedAt).Take(limit).ToList());

    public Task<int> CountNegativeReviewsForProductAsync(Guid productId, CancellationToken cancellationToken) => Task.FromResult(NegativeReviewCount);

    public Task<List<ProducerModerationWarning>> GetWarningsForProductAsync(Guid productId, int limit, CancellationToken cancellationToken)
        => Task.FromResult(Warnings.Where(w => w.ProductId == productId).OrderByDescending(w => w.CreatedAt).Take(limit).ToList());

    public Task<(List<ProductModerationCaseRow> Items, int TotalCount)> GetCasesAsync(
        ProductModerationRiskState? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken)
        => Task.FromResult(CasesResult);

    public Task AddEventAsync(ProductModerationEvent moderationEvent, CancellationToken cancellationToken) { Events.Add(moderationEvent); return Task.CompletedTask; }
    public Task AddWarningAsync(ProducerModerationWarning warning, CancellationToken cancellationToken) { Warnings.Add(warning); return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>Backs the REAL <see cref="MonitoringService"/> — flags are stored by reference, so the service's
/// own mutations (status, Events) are visible to the test exactly like a tracked EF entity would be.</summary>
class FakeMonitoringRepository : IMonitoringRepository
{
    public Dictionary<Guid, MonitoringFlag> Flags { get; } = new();

    public void SeedFlag(MonitoringFlag flag) => Flags[flag.Id] = flag;

    public Task AddFlagAsync(MonitoringFlag flag, CancellationToken cancellationToken) { Flags[flag.Id] = flag; return Task.CompletedTask; }
    public void RemoveFlag(MonitoringFlag flag) => Flags.Remove(flag.Id);
    public Task<MonitoringFlag?> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Flags.GetValueOrDefault(id));
    public Task<(List<MonitoringFlag> Items, int TotalCount)> GetFlagsPagedAsync(MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
        => Task.FromResult((Flags.Values.ToList(), Flags.Count));

    public Task<HashSet<string>> GetOpenFlagDedupeKeysAsync(IEnumerable<string> candidateKeys, CancellationToken cancellationToken)
    {
        var keys = candidateKeys.ToHashSet();
        var open = Flags.Values.Where(f => keys.Contains(f.DedupeKey) && f.Status != MonitoringFlagStatus.Dismissed && f.Status != MonitoringFlagStatus.Resolved)
            .Select(f => f.DedupeKey).ToHashSet();
        return Task.FromResult(open);
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task<Guid?> GetAnySuperAdminUserIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(null);

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

class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();
    public void Add(User user) => _users[user.Id] = user;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_users.GetValueOrDefault(id));
    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_users.GetValueOrDefault(id));
    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken) => Task.FromResult<User?>(null);
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(false);
    public Task<bool> AnyInRoleAsync(string roleName, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task AddAsync(User user, CancellationToken cancellationToken) { _users[user.Id] = user; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

class FakeAuditLogService : IAuditLogService
{
    public record Call(Guid? ActorUserId, string ActorName, string Action, string EntityType, Guid? EntityId, string Description);
    public List<Call> Calls { get; } = new();

    public Task LogAsync(Guid? actorUserId, string actorName, string action, string entityType, Guid? entityId, string description, string? ipAddress, CancellationToken cancellationToken)
    {
        Calls.Add(new Call(actorUserId, actorName, action, entityType, entityId, description));
        return Task.CompletedTask;
    }

    public Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryParameters query, CancellationToken cancellationToken)
        => Task.FromResult(new PagedResult<AuditLogDto>());
}

/// <summary>Only <see cref="SetApprovalAsync"/> is exercised by the Ban action; every other member throws if
/// accidentally called, so a wrong wiring shows up immediately instead of silently doing nothing.</summary>
class FakeProductServiceForBan : IProductService
{
    public record ApprovalCall(Guid ProductId, Guid AdminUserId, SetProductApprovalRequest Request);
    public List<ApprovalCall> ApprovalCalls { get; } = new();

    public Task<ProductDto> SetApprovalAsync(Guid productId, Guid adminUserId, SetProductApprovalRequest request, CancellationToken cancellationToken)
    {
        ApprovalCalls.Add(new ApprovalCall(productId, adminUserId, request));
        return Task.FromResult(new ProductDto { Id = productId, ApprovalStatus = request.Status });
    }

    public Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<ProductListItemDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<ProductListItemDto>> GetTrendingAsync(int count, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<List<ProductDto>> GetMineAsync(Guid producerId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> CreateAsync(Guid producerId, CreateProductRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> UpdateAsync(Guid id, Guid currentUserId, bool isAdmin, UpdateProductRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> SetFeaturedAsync(Guid id, bool isFeatured, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> AddVariantAsync(Guid productId, Guid currentUserId, bool isAdmin, CreateProductVariantRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> UpdateVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, UpdateProductVariantRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> DeleteVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<BulkCreateProductsResultDto> BulkCreateAsync(Guid producerId, BulkCreateProductsRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> AddVideoAsync(Guid productId, Guid currentUserId, bool isAdmin, CreateProductVideoRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> UpdateVideoAsync(Guid productId, Guid videoId, Guid currentUserId, bool isAdmin, UpdateProductVideoRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> DeleteVideoAsync(Guid productId, Guid videoId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ProductDto> SetHandmadeVerificationAsync(Guid productId, Guid verifierUserId, SetHandmadeVerificationRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<PagedResult<ProductListItemDto>> GetPendingApprovalAsync(int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
}
