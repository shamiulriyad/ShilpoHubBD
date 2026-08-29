using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPickupSchedulingService
{
    Task<PickupRequestDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreatePickupRequestRequest request, CancellationToken cancellationToken);

    Task<PagedResult<PickupRequestListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, PickupRequestQueryParameters query, CancellationToken cancellationToken);

    Task<PickupRequestDto> GetByIdAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<PickupRequestDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdatePickupRequestRequest request, CancellationToken cancellationToken);

    Task<PickupRequestDto> ScheduleAsync(
        Guid currentUserId, bool isAdmin, Guid id, SchedulePickupRequestRequest request, CancellationToken cancellationToken);

    Task<PickupRequestDto> AssignAsync(
        Guid currentUserId, bool isAdmin, Guid id, AssignPickupRequestRequest request, CancellationToken cancellationToken);

    Task<PickupRequestDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdatePickupStatusRequest request, CancellationToken cancellationToken);

    Task<PickupRequestDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelPickupRequestRequest request, CancellationToken cancellationToken);

    Task<PickupRequestDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddPickupNoteRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
