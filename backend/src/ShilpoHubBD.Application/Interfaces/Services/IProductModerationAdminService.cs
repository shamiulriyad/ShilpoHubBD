using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// The Admin side of product-review moderation (Part 3): the case list/detail an admin needs to decide a case,
/// and the Ban action. AI never bans a product — this service only ever runs on an explicit admin request, and
/// every write it makes goes through EXISTING infrastructure (product approval status, audit log, the generic
/// flag-status workflow, the existing notification mechanism) rather than a parallel one.
/// </summary>
public interface IProductModerationAdminService
{
    Task<PagedResult<ProductModerationCaseListItemDto>> GetCasesAsync(
        string? riskState, string? status, int page, int pageSize, CancellationToken cancellationToken);

    Task<ProductModerationCaseDto> GetCaseDetailAsync(Guid flagId, CancellationToken cancellationToken);

    /// <summary>Only ever called from an admin-authorized request (enforced by the controller). Rejects the
    /// product via the existing approval-status logic, marks the moderation case, resolves the flag, and
    /// records an audit entry — the producer notification is a side effect of the existing status change.</summary>
    Task<MonitoringFlagDto> BanProductAsync(Guid adminUserId, Guid flagId, BanProductRequest request, CancellationToken cancellationToken);
}
