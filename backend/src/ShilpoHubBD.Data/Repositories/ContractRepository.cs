using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Contracts;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Data.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ShilpoHubDbContext _context;

    public ContractRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Contract> WithDetails()
        => _context.Contracts
            .Include(c => c.BusinessPartner)
            .Include(c => c.Producer)
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.DeliverySchedules)
            .Include(c => c.Documents)
            .Include(c => c.StatusHistory)
            .AsSplitQuery();

    private IQueryable<Contract> ForListing()
        => _context.Contracts
            .Include(c => c.Producer)
            .Include(c => c.Items)
            .AsSplitQuery();

    public Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<ContractDeliverySchedule?> GetDeliveryScheduleAsync(Guid contractId, Guid scheduleId, CancellationToken cancellationToken)
        => _context.ContractDeliverySchedules
            .Include(s => s.Contract)
            .FirstOrDefaultAsync(s => s.Id == scheduleId && s.ContractId == contractId, cancellationToken);

    private static async Task<(List<Contract> Items, int TotalCount)> PageAsync(
        IQueryable<Contract> query, ContractQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Status.HasValue)
        {
            query = query.Where(c => c.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<(List<Contract> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, ContractQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(c => c.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<Contract> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, ContractQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(c => c.ProducerId == producerId), parameters, cancellationToken);

    public Task<(List<Contract> Items, int TotalCount)> GetPagedAllAsync(
        ContractQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.Contracts.AnyAsync(c => c.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken)
        => await _context.Contracts.AddAsync(contract, cancellationToken);

    public async Task AddStatusEventAsync(ContractStatusEvent statusEvent, CancellationToken cancellationToken)
        => await _context.ContractStatusEvents.AddAsync(statusEvent, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
