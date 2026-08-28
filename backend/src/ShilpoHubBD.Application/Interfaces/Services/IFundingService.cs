using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IFundingService
{
    // ---- Programs -----------------------------------------------------
    Task<FundingProgramDto> CreateProgramAsync(
        Guid userId, CreateFundingProgramRequest request, CancellationToken cancellationToken);

    Task<PagedResult<FundingProgramListItemDto>> GetProgramsAsync(
        FundingProgramQueryParameters query, CancellationToken cancellationToken);

    Task<FundingProgramDto> GetProgramByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FundingProgramDto> UpdateProgramAsync(
        Guid userId, Guid id, UpdateFundingProgramRequest request, CancellationToken cancellationToken);

    Task DeleteProgramAsync(Guid id, CancellationToken cancellationToken);

    // ---- Applications ----------------------------------------------
    Task<FundingApplicationDto> CreateApplicationAsync(
        Guid userId, CreateFundingApplicationRequest request, CancellationToken cancellationToken);

    Task<PagedResult<FundingApplicationListItemDto>> GetApplicationsAsync(
        FundingApplicationQueryParameters query, CancellationToken cancellationToken);

    Task<FundingApplicationDto> GetApplicationByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FundingApplicationDto> AddReviewAsync(
        Guid userId, Guid id, SubmitFundingReviewRequest request, CancellationToken cancellationToken);

    Task<FundingApplicationDto> DecideAsync(
        Guid userId, Guid id, DecideFundingApplicationRequest request, CancellationToken cancellationToken);

    Task<FundingApplicationDto> WithdrawAsync(
        Guid userId, Guid id, WithdrawFundingApplicationRequest request, CancellationToken cancellationToken);

    Task<FundingApplicationDto> AddNoteAsync(
        Guid userId, Guid id, AddFundingApplicationNoteRequest request, CancellationToken cancellationToken);

    Task<FundingApplicationDto> ScheduleDisbursementAsync(
        Guid userId, Guid id, ScheduleFundingDisbursementRequest request, CancellationToken cancellationToken);

    Task<FundingApplicationDto> UpdateDisbursementStatusAsync(
        Guid userId, Guid id, Guid disbursementId, UpdateFundingDisbursementStatusRequest request,
        CancellationToken cancellationToken);

    Task<FundingApplicationDto> RecordRepaymentAsync(
        Guid userId, Guid id, RecordLoanRepaymentRequest request, CancellationToken cancellationToken);

    Task DeleteApplicationAsync(Guid id, CancellationToken cancellationToken);
}
