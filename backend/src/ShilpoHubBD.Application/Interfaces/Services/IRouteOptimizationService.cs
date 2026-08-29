using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IRouteOptimizationService
{
    Task<DeliveryRouteDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateDeliveryRouteRequest request, CancellationToken cancellationToken);

    Task<PagedResult<DeliveryRouteListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, DeliveryRouteQueryParameters query, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> GetByIdAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateDeliveryRouteRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> AddStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteStopInput request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> UpdateStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, UpdateRouteStopRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> RemoveStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> ResequenceAsync(
        Guid currentUserId, bool isAdmin, Guid id, ResequenceRouteRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> OptimizeAsync(
        Guid currentUserId, bool isAdmin, Guid id, OptimizeRouteRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> AssignAsync(
        Guid currentUserId, bool isAdmin, Guid id, AssignRouteRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> DispatchAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> StartAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> CompleteAsync(
        Guid currentUserId, bool isAdmin, Guid id, RouteTransitionRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelRouteRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> ArriveStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> CompleteStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, CompleteRouteStopRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> SkipStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> FailStopAsync(
        Guid currentUserId, bool isAdmin, Guid id, Guid stopId, FailRouteStopRequest request, CancellationToken cancellationToken);

    Task<DeliveryRouteDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddRouteNoteRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
