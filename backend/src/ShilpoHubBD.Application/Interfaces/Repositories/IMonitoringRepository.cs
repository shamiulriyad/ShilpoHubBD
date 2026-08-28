using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>
/// Persistence for <see cref="MonitoringFlag"/>s plus the read queries that back the rule-based
/// fraud / fake-product / review-abuse / QR-anomaly scans and the QR monitoring overview.
/// </summary>
public interface IMonitoringRepository
{
    // ---- Flags ---------------------------------------------------------
    Task AddFlagAsync(MonitoringFlag flag, CancellationToken cancellationToken);

    void RemoveFlag(MonitoringFlag flag);

    Task<MonitoringFlag?> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<MonitoringFlag> Items, int TotalCount)> GetFlagsPagedAsync(
        MonitoringFlagQueryParameters query, CancellationToken cancellationToken);

    Task<HashSet<string>> GetOpenFlagDedupeKeysAsync(
        IEnumerable<string> candidateKeys, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    // ---- Scan signal queries ----------------------------------------
    Task<List<ScanCandidate>> FindFraudCandidatesAsync(DateTime since, CancellationToken cancellationToken);

    Task<List<ScanCandidate>> FindFakeProductCandidatesAsync(DateTime since, CancellationToken cancellationToken);

    Task<List<ScanCandidate>> FindReviewAbuseCandidatesAsync(DateTime since, CancellationToken cancellationToken);

    Task<List<ScanCandidate>> FindQrAnomalyCandidatesAsync(DateTime since, CancellationToken cancellationToken);

    // ---- QR monitoring overview ----------------------------------
    Task<QrMonitoringOverviewDto> GetQrOverviewAsync(
        DateTime? from, DateTime? to, int topN, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>A finding produced by a monitoring scan, before it is turned into a flag.</summary>
public record ScanCandidate(
    MonitoringFlagType FlagType,
    MonitoringFlagSeverity Severity,
    MonitoringSubjectType SubjectType,
    Guid? SubjectId,
    string SubjectLabel,
    string Title,
    string Description,
    decimal RiskScore,
    string EvidenceJson,
    string DedupeKey);
