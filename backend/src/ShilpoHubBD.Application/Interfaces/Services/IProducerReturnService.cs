using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerReturnService
{
    Task<List<ProducerReturnDto>> GetReturnsAsync(Guid producerId, CancellationToken cancellationToken);
    Task<ProducerReturnDto> AcceptAsync(Guid producerId, Guid orderId, CancellationToken cancellationToken);
    Task<ProducerReturnDto> RejectAsync(Guid producerId, Guid orderId, string? note, CancellationToken cancellationToken);
}
