using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IDeliveryTrackingService
{
    Task<ShipmentDto> CreateAsync(
        Guid currentUserId, bool isAdmin, CreateShipmentRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ShipmentListItemDto>> GetPagedAsync(
        Guid currentUserId, bool isAdmin, ShipmentQueryParameters query, CancellationToken cancellationToken);

    Task<ShipmentDto> GetByIdAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);

    /// <summary>Public, PII-light tracking lookup by tracking number. No authorization.</summary>
    Task<ShipmentTrackingDto> TrackByNumberAsync(string trackingNumber, CancellationToken cancellationToken);

    Task<ShipmentDto> UpdateAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> UpdateStatusAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentStatusRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> AddTrackingEventAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddShipmentTrackingEventRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> UpdateLocationAsync(
        Guid currentUserId, bool isAdmin, Guid id, UpdateShipmentLocationRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> RecordDeliveryAttemptAsync(
        Guid currentUserId, bool isAdmin, Guid id, RecordDeliveryAttemptRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> MarkDeliveredAsync(
        Guid currentUserId, bool isAdmin, Guid id, MarkShipmentDeliveredRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> CancelAsync(
        Guid currentUserId, bool isAdmin, Guid id, CancelShipmentRequest request, CancellationToken cancellationToken);

    Task<ShipmentDto> AddNoteAsync(
        Guid currentUserId, bool isAdmin, Guid id, AddShipmentNoteRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid currentUserId, bool isAdmin, Guid id, CancellationToken cancellationToken);
}
