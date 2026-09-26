using ShilpoHubBD.Application.DTOs.Complaints;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IOrderComplaintService
{
    Task<OrderComplaintDto> CreateAsync(Guid customerId, CreateOrderComplaintRequest request, CancellationToken cancellationToken);
    Task<List<OrderComplaintDto>> GetMineAsCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<List<OrderComplaintDto>> GetMineAsProducerAsync(Guid producerId, CancellationToken cancellationToken);

    // Producer says it is sorted out.
    Task<OrderComplaintDto> RespondAsync(Guid id, Guid producerId, RespondToOrderComplaintRequest request, CancellationToken cancellationToken);

    // Customer decides.
    Task<OrderComplaintDto> ConfirmSatisfiedAsync(Guid id, Guid customerId, CustomerComplaintNoteRequest request, CancellationToken cancellationToken);
    Task<OrderComplaintDto> ReopenAsync(Guid id, Guid customerId, CustomerComplaintNoteRequest request, CancellationToken cancellationToken);
    Task<OrderComplaintDto> WithdrawAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
}
