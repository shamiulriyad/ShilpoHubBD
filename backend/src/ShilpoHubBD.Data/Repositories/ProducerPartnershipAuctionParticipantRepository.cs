using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerPartnershipAuctionParticipantRepository : IProducerPartnershipAuctionParticipantRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerPartnershipAuctionParticipantRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<ProducerPartnershipAuctionParticipant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionParticipants
            .Include(p => p.BusinessPartner)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ProducerPartnershipAuctionParticipant?> GetForBusinessPartnerAsync(
        Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionParticipants
            .FirstOrDefaultAsync(p => p.AuctionId == auctionId && p.BusinessPartnerId == businessPartnerId, cancellationToken);

    public Task<List<ProducerPartnershipAuctionParticipant>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAuctionParticipants
            .Include(p => p.BusinessPartner)
            .Where(p => p.AuctionId == auctionId)
            .OrderBy(p => p.AppliedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ProducerPartnershipAuctionParticipant participant, CancellationToken cancellationToken)
        => await _context.ProducerPartnershipAuctionParticipants.AddAsync(participant, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
