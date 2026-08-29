using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IReturnHandlingService
{
    Task<ReturnRequestDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateReturnRequestRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ReturnRequestListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, ReturnRequestQueryParameters query, CancellationToken cancellationToken);

    Task<ReturnRequestDto> GetByIdAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<ReturnRequestDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateReturnRequestRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> ApproveAsync(
        Guid currentUserId, bool isAdmin, Guid id, ApproveReturnRequestRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> RejectAsync(
        Guid currentUserId, bool isAdmin, Guid id, RejectReturnRequestRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> SchedulePickupAsync(
        Guid currentUserId, bool isAdmin, Guid id, ScheduleReturnPickupRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateReturnStatusRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> RecordInspectionAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordReturnInspectionRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> RestockAsync(
        Guid currentUserId, bool isAdmin, Guid id, RestockReturnRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> RecordRefundAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordReturnRefundRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> CloseAsync(
        Guid currentUserId, bool isAdmin, Guid id, CloseReturnRequestRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelReturnRequestRequest request, CancellationToken cancellationToken);

    Task<ReturnRequestDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddReturnNoteRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
