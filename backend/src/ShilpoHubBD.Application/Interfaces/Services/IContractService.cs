using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IContractService
{
    Task<ContractDto> CreateAsync(Guid businessPartnerId, CreateContractRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ContractListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, ContractQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<ContractListItemDto>> GetForProducerAsync(Guid producerId, ContractQueryParameters parameters, CancellationToken cancellationToken);
    Task<ContractDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<ContractDto> AcceptAsync(Guid id, Guid producerId, CancellationToken cancellationToken);
    Task<ContractDto> RejectAsync(Guid id, Guid producerId, ContractDecisionRequest request, CancellationToken cancellationToken);
    Task<ContractDto> TerminateAsync(Guid id, Guid currentUserId, bool isAdmin, ContractDecisionRequest request, CancellationToken cancellationToken);
    Task<ContractDto> RenewAsync(Guid id, Guid businessPartnerId, bool isAdmin, RenewContractRequest request, CancellationToken cancellationToken);

    Task<ContractDocumentDto> AddDocumentAsync(Guid id, Guid currentUserId, bool isAdmin, AddContractDocumentRequest request, CancellationToken cancellationToken);
    Task<ContractDeliveryScheduleDto> UpdateDeliveryStatusAsync(Guid id, Guid scheduleId, Guid currentUserId, bool isAdmin, UpdateDeliveryStatusRequest request, CancellationToken cancellationToken);
}
