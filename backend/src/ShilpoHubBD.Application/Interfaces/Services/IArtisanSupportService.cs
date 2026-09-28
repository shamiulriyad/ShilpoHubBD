using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IArtisanSupportService
{
    Task<SupportOrganizationDto?> GetMyOrganizationAsync(Guid userId, CancellationToken ct);
    Task<SupportOrganizationDto> UpsertOrganizationAsync(Guid userId, UpsertSupportOrganizationRequest request, CancellationToken ct);
    Task<IReadOnlyList<SupportOrganizationDto>> GetOrganizationsAsync(CancellationToken ct);
    Task<SupportOrganizationDto> ReviewOrganizationAsync(Guid adminId, Guid id, ReviewSupportOrganizationRequest request, CancellationToken ct);
    Task<IReadOnlyList<SupportUserOptionDto>> GetArtisansAsync(CancellationToken ct);
    Task<IReadOnlyList<SupportUserOptionDto>> GetApprovedOrganizationsAsync(CancellationToken ct);
    Task<IReadOnlyList<ArtisanSupportCaseDto>> GetCasesAsync(Guid userId, bool isAdmin, bool isGovernment, CancellationToken ct);
    Task<ArtisanSupportCaseDto> GetCaseAsync(Guid userId, Guid id, bool isAdmin, bool isGovernment, CancellationToken ct);
    Task<ArtisanSupportCaseDto> CreateCaseAsync(Guid userId, bool isGovernment, CreateArtisanSupportCaseRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> AssignAsync(Guid adminId, Guid id, AssignArtisanSupportCaseRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> AcceptAsync(Guid userId, Guid id, CancellationToken ct);
    Task<ArtisanSupportCaseDto> InspectAsync(Guid userId, Guid id, RecordSupportInspectionRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> PlanAsync(Guid userId, Guid id, RecordSupportPlanRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> RecordProvidedAsync(Guid userId, Guid id, RecordSupportProvidedRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> ConfirmAsync(Guid artisanId, Guid id, ConfirmArtisanSupportRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> MonitorAsync(Guid userId, Guid id, AddSupportMonitoringRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> SubmitReportAsync(Guid userId, Guid id, SubmitSupportReportRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> ReviewReportAsync(Guid adminId, Guid id, ReviewSupportReportRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> FlagAsync(Guid adminId, Guid id, FlagSupportCaseRequest request, CancellationToken ct);
    Task<ArtisanSupportCaseDto> AddEvidenceAsync(Guid userId, Guid id, bool isAdmin, CreateSupportEvidenceRequest request, CancellationToken ct);
    Task<ArtisanSupportDashboardDto> DashboardAsync(Guid userId, bool isAdmin, bool isGovernment, CancellationToken ct);
}
