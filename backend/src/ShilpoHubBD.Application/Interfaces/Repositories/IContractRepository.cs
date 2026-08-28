using ShilpoHubBD.Application.DTOs.Contracts;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ContractDeliverySchedule?> GetDeliveryScheduleAsync(Guid contractId, Guid scheduleId, CancellationToken cancellationToken);
    Task<(List<Contract> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ContractQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<Contract> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, ContractQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<Contract> Items, int TotalCount)> GetPagedAllAsync(ContractQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(Contract contract, CancellationToken cancellationToken);
    Task AddStatusEventAsync(ContractStatusEvent statusEvent, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
