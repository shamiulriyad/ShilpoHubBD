using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerMonthlyReportRepository
{
    Task<bool> ExistsAsync(Guid producerId, int year, int month, CancellationToken cancellationToken);
    Task<ProducerMonthlyReport?> GetAsync(Guid producerId, int year, int month, CancellationToken cancellationToken);

    /// <summary>Every report for this (Year, Month) — the peer cohort used for positioning. Tracked, so entities also present in the change tracker (freshly-added ones) are returned by reference, not re-fetched.</summary>
    Task<List<ProducerMonthlyReport>> GetAllForMonthAsync(int year, int month, CancellationToken cancellationToken);

    /// <summary>Every user holding the Producer role.</summary>
    Task<List<Guid>> GetProducerIdsAsync(CancellationToken cancellationToken);

    Task AddAsync(ProducerMonthlyReport report, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);

    // ---- Admin read/search ----------------------------------------------

    /// <summary>Single report by its own Id, with Producer/Category/District loaded for display.</summary>
    Task<ProducerMonthlyReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<ProducerMonthlyReport> Items, int TotalCount)> GetPagedAsync(
        ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken);

    /// <summary>Every report matching the filters (Page/PageSize ignored) — for dashboard aggregates over the whole filtered set, not just one page.</summary>
    Task<List<ProducerMonthlyReport>> GetAllMatchingAsync(ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken);

    /// <summary>Single report by producer + month, with Producer/Category/District loaded for display.</summary>
    Task<ProducerMonthlyReport?> GetForProducerWithDetailsAsync(Guid producerId, int year, int month, CancellationToken cancellationToken);

    /// <summary>The producer's most recent report (highest Year, then Month), with details loaded. Null if the producer has never had a report generated.</summary>
    Task<ProducerMonthlyReport?> GetLatestForProducerAsync(Guid producerId, CancellationToken cancellationToken);

    // ---- Sharing ----------------------------------------------------------

    Task<bool> ReportExistsAsync(Guid reportId, CancellationToken cancellationToken);

    /// <summary>Of the given ids, the ones that hold the given role.</summary>
    Task<List<Guid>> FilterByRoleAsync(IEnumerable<Guid> userIds, string roleName, CancellationToken cancellationToken);

    Task<bool> IsSharedWithAsync(Guid reportId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Of the given SharedWithUserIds, the ones that already have a share row for this report.</summary>
    Task<List<Guid>> GetExistingShareUserIdsAsync(Guid reportId, IEnumerable<Guid> sharedWithUserIds, CancellationToken cancellationToken);

    Task AddShareAsync(ProducerMonthlyReportShare share, CancellationToken cancellationToken);

    /// <summary>Every share for this report, with SharedWithUser/SharedByUser loaded for display.</summary>
    Task<List<ProducerMonthlyReportShare>> GetSharesForReportAsync(Guid reportId, CancellationToken cancellationToken);
}
