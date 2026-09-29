using System.Text.Json;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Governance;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Services.Reviews;

public class ProductModerationAdminService : IProductModerationAdminService
{
    private const int HistoryLimit = 30;

    private readonly IProductModerationRepository _repository;
    private readonly IMonitoringRepository _monitoringRepository;
    private readonly IMonitoringService _monitoringService;
    private readonly IProductService _productService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;

    public ProductModerationAdminService(
        IProductModerationRepository repository,
        IMonitoringRepository monitoringRepository,
        IMonitoringService monitoringService,
        IProductService productService,
        IAuditLogService auditLogService,
        IUserRepository userRepository)
    {
        _repository = repository;
        _monitoringRepository = monitoringRepository;
        _monitoringService = monitoringService;
        _productService = productService;
        _auditLogService = auditLogService;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ProductModerationCaseListItemDto>> GetCasesAsync(
        string? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        ProductModerationRiskState? parsedRiskState = !string.IsNullOrWhiteSpace(riskState)
            && Enum.TryParse<ProductModerationRiskState>(riskState, true, out var rs) ? rs : null;

        var (items, totalCount) = await _repository.GetCasesAsync(parsedRiskState, status, page, pageSize, cancellationToken);

        return new PagedResult<ProductModerationCaseListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(r => new ProductModerationCaseListItemDto
            {
                FlagId = r.FlagId,
                ProductId = r.ProductId,
                ProductName = r.ProductName,
                ProducerId = r.ProducerId,
                ProducerName = r.ProducerName,
                RiskState = r.RiskState.ToString(),
                SimilarComplaintCount = r.SimilarComplaintCount,
                HighSeverityComplaintCount = r.HighSeverityComplaintCount,
                ProducerWarningCount = r.ProducerWarningCount,
                ModerationStatus = r.ModerationStatus,
                DetectedAt = r.DetectedAt,
            }).ToList(),
        };
    }

    public async Task<ProductModerationCaseDto> GetCaseDetailAsync(Guid flagId, CancellationToken cancellationToken)
    {
        var flag = await LoadProductFlagAsync(flagId, cancellationToken);
        var productId = flag.SubjectId!.Value;

        var state = await _repository.GetOrCreateStateAsync(productId, cancellationToken);
        var negativeReviews = await _repository.CountNegativeReviewsForProductAsync(productId, cancellationToken);
        var warnings = await _repository.GetWarningsForProductAsync(productId, HistoryLimit, cancellationToken);
        var recentEvents = await _repository.GetRecentEventsAsync(productId, HistoryLimit, cancellationToken);

        var evidence = ParseEvidence(flag.EvidenceJson);

        var idsToHydrate = new List<Guid>();
        if (evidence.TriggeringReviewId is { } triggeringId)
        {
            idsToHydrate.Add(triggeringId);
        }

        idsToHydrate.AddRange(evidence.SimilarReviewIds);
        var hydrated = idsToHydrate.Count > 0
            ? await _repository.GetReviewsWithAnalysisByIdsAsync(idsToHydrate, cancellationToken)
            : new Dictionary<Guid, (Review Review, ReviewAiAnalysis? Analysis)>();

        ReviewAiAnalysis? triggeringAnalysis = null;
        ModerationReviewDto? triggeringReview = null;
        if (evidence.TriggeringReviewId is { } trId && hydrated.TryGetValue(trId, out var triggering))
        {
            triggeringAnalysis = triggering.Analysis;
            triggeringReview = ToReviewDto(triggering.Review, triggering.Analysis);
        }

        var similarReviews = evidence.SimilarReviewIds
            .Where(id => id != evidence.TriggeringReviewId && hydrated.ContainsKey(id))
            .Select(id => ToReviewDto(hydrated[id].Review, hydrated[id].Analysis))
            .ToList();

        var history = BuildHistory(recentEvents, warnings, flag.Events);

        return new ProductModerationCaseDto
        {
            FlagId = flag.Id,
            FlagStatus = flag.Status.ToString(),
            FlagSeverity = flag.Severity.ToString(),
            DetectedAt = flag.DetectedAt,

            ProductId = productId,
            ProductName = state.Product.Name,
            ProductSlug = state.Product.Slug,
            ProductApprovalStatus = state.Product.ApprovalStatus.ToString(),

            ProducerId = state.Product.ProducerId,
            ProducerName = state.Product.Producer?.FullName ?? string.Empty,
            ProducerEmail = state.Product.Producer?.Email ?? string.Empty,

            TotalReviews = state.Product.ReviewCount,
            NegativeReviews = negativeReviews,
            SimilarComplaints = state.SimilarComplaintCount,
            HighSeverityComplaints = state.HighSeverityComplaintCount,
            PreviousWarnings = state.ProducerWarningCount,
            RiskState = state.RiskState.ToString(),

            TriggeringReview = triggeringReview,
            SimilarReviews = similarReviews,

            IssueSummary = triggeringAnalysis?.IssueSummary,
            IsRepeatedComplaint = evidence.IsRepeatedComplaint,
            AiComplaintType = evidence.ComplaintType,
            AiSeverity = evidence.Severity,
            AiConfidence = evidence.Confidence,
            AiExplanation = evidence.AiExplanation,
            IsAiGenerated = evidence.IsAiGenerated,

            History = history,
        };
    }

    public async Task<MonitoringFlagDto> BanProductAsync(Guid adminUserId, Guid flagId, BanProductRequest request, CancellationToken cancellationToken)
    {
        var flag = await LoadProductFlagAsync(flagId, cancellationToken);
        var productId = flag.SubjectId!.Value;

        var admin = await _userRepository.GetByIdAsync(adminUserId, cancellationToken)
            ?? throw new NotFoundException("Admin user not found.");

        var reason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Banned by admin following repeated product-complaint moderation."
            : request.Reason.Trim();

        // Update the product's status via the EXISTING product-approval logic. This already audit-logs
        // ("Product.Rejected") and already notifies the producer (the Product/ApprovalStatus branch in
        // ShilpoHubDbContext.Notifications.cs fires whenever ApprovalStatus changes) — no new status system,
        // no new audit system, no new notification path.
        await _productService.SetApprovalAsync(productId, adminUserId, new SetProductApprovalRequest
        {
            Status = ProductApprovalStatus.Rejected,
            RejectionReason = reason,
        }, cancellationToken);

        // Mark the moderation case (Part 2's own history table).
        var state = await _repository.GetOrCreateStateAsync(productId, cancellationToken);
        await _repository.AddEventAsync(new ProductModerationEvent
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            // Product-level admin action, not tied to one review; falls back to the last review this case
            // was evaluated against (always set by the time a HighRisk flag exists).
            ReviewId = state.LastReviewId ?? Guid.Empty,
            Type = ProductModerationEventType.ProductBanned,
            ComplaintType = ReviewComplaintType.None,
            Severity = ReviewSeverity.None,
            Reason = reason,
            Confidence = 0,
            IsAiGenerated = false,
            SimilarReviewIds = new List<Guid>(),
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Resolve the flag through the EXISTING flag-status workflow (same one Dismiss/Keep Monitoring use).
        if (flag.Status is not (MonitoringFlagStatus.Resolved or MonitoringFlagStatus.Dismissed))
        {
            await _monitoringService.UpdateFlagStatusAsync(adminUserId, flagId, new UpdateMonitoringFlagStatusRequest
            {
                Status = nameof(MonitoringFlagStatus.Resolved),
                Note = $"Product banned. {reason}",
            }, cancellationToken);
        }

        // A second, purpose-specific audit entry via the SAME existing audit service/table (not a duplicate
        // system) — SetApprovalAsync's own "Product.Rejected" entry doesn't say this was a moderation ban.
        await _auditLogService.LogAsync(adminUserId, admin.FullName, "Product.Banned", "Product", productId,
            $"Banned product \"{state.Product.Name}\" via moderation case {flagId}. Reason: {reason}.", null, cancellationToken);

        var updated = await _monitoringRepository.GetFlagByIdAsync(flagId, cancellationToken);
        return updated!.ToDto();
    }

    // ---- helpers -----------------------------------------------------------------------------------------------

    private async Task<MonitoringFlag> LoadProductFlagAsync(Guid flagId, CancellationToken cancellationToken)
    {
        var flag = await _monitoringRepository.GetFlagByIdAsync(flagId, cancellationToken)
            ?? throw new NotFoundException("Moderation case not found.");

        if (flag.SubjectType != MonitoringSubjectType.Product || flag.SubjectId is null)
        {
            throw new ConflictException("This flag is not a product-moderation case.");
        }

        return flag;
    }

    private static ModerationReviewDto ToReviewDto(Review review, ReviewAiAnalysis? analysis) => new()
    {
        ReviewId = review.Id,
        Text = review.Comment,
        Rating = review.Rating,
        CreatedAt = review.CreatedAt,
        ComplaintType = analysis?.ComplaintType.ToString(),
        Severity = analysis?.Severity.ToString(),
        Similarity = null,
    };

    private static List<ModerationHistoryItemDto> BuildHistory(
        List<ProductModerationEvent> events, List<ProducerModerationWarning> warnings, ICollection<MonitoringFlagEvent> flagEvents)
    {
        var items = new List<ModerationHistoryItemDto>();
        items.AddRange(events.Select(e => new ModerationHistoryItemDto
        {
            Kind = "ModerationEvent",
            Type = e.Type.ToString(),
            Note = e.Reason,
            ActorName = null,
            CreatedAt = e.CreatedAt,
        }));
        items.AddRange(warnings.Select(w => new ModerationHistoryItemDto
        {
            Kind = "Warning",
            Type = "ProducerWarning",
            Note = w.Message,
            ActorName = null,
            CreatedAt = w.CreatedAt,
        }));
        items.AddRange(flagEvents.Select(e => new ModerationHistoryItemDto
        {
            Kind = "AdminAction",
            Type = e.Type.ToString(),
            Note = e.Note,
            ActorName = e.Actor?.FullName,
            CreatedAt = e.CreatedAt,
        }));

        return items.OrderByDescending(i => i.CreatedAt).Take(HistoryLimit).ToList();
    }

    private static EvidenceSnapshot ParseEvidence(string? evidenceJson)
    {
        if (string.IsNullOrWhiteSpace(evidenceJson))
        {
            return new EvidenceSnapshot(null, new List<Guid>(), null, null, null, null, null);
        }

        try
        {
            using var doc = JsonDocument.Parse(evidenceJson);
            var root = doc.RootElement;

            Guid? triggeringReviewId = root.TryGetProperty("triggeringReviewId", out var trEl) && trEl.TryGetGuid(out var trId) ? trId : null;

            var similarIds = new List<Guid>();
            if (root.TryGetProperty("similarHistoricalReviewIds", out var simEl) && simEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in simEl.EnumerateArray())
                {
                    if (item.TryGetGuid(out var id))
                    {
                        similarIds.Add(id);
                    }
                }
            }

            string? complaintType = root.TryGetProperty("complaintType", out var ctEl) ? ctEl.GetString() : null;
            string? severity = root.TryGetProperty("severity", out var sevEl) ? sevEl.GetString() : null;
            string? aiExplanation = root.TryGetProperty("aiExplanation", out var expEl) ? expEl.GetString() : null;
            bool? isAiGenerated = root.TryGetProperty("isAiGenerated", out var aiEl) && aiEl.ValueKind is JsonValueKind.True or JsonValueKind.False ? aiEl.GetBoolean() : null;
            bool? isRepeatedComplaint = root.TryGetProperty("isRepeatedComplaint", out var repEl) && repEl.ValueKind is JsonValueKind.True or JsonValueKind.False ? repEl.GetBoolean() : null;
            double? confidence = root.TryGetProperty("confidence", out var confEl) && confEl.ValueKind == JsonValueKind.Number ? confEl.GetDouble() : null;

            return new EvidenceSnapshot(triggeringReviewId, similarIds, complaintType, severity, aiExplanation, isAiGenerated, isRepeatedComplaint) { Confidence = confidence };
        }
        catch (JsonException)
        {
            return new EvidenceSnapshot(null, new List<Guid>(), null, null, null, null, null);
        }
    }

    private record EvidenceSnapshot(
        Guid? TriggeringReviewId, List<Guid> SimilarReviewIds, string? ComplaintType, string? Severity,
        string? AiExplanation, bool? IsAiGenerated, bool? IsRepeatedComplaint)
    {
        public double? Confidence { get; init; }
    }
}
