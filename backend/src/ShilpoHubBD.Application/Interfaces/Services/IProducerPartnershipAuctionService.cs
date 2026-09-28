using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Admin configuration and lifecycle for the annual Producer Partnership Auction. Settlement
/// (revenue-share computation, agreement creation from a won lot) is separate, later work — this
/// only carries an auction through Draft → Scheduled → RegistrationOpen → Live → Ended/Cancelled and
/// determines each lot's winner (subject to MaxProducersPerBusinessPartner) when it ends.
/// </summary>
public interface IProducerPartnershipAuctionService
{
    Task<ProducerPartnershipAuctionDto> CreateAsync(Guid managedByUserId, CreateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> UpdateAsync(Guid id, UpdateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ProducerPartnershipAuctionListItemDto>> GetPagedAsync(ProducerPartnershipAuctionQueryParameters parameters, CancellationToken cancellationToken);

    Task<ProducerPartnershipAuctionDto> ScheduleAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> OpenRegistrationAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> GoLiveAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> EndAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
