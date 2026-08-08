using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.DTOs.Common;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAuctionService
{
    Task<PagedResult<AuctionListItemDto>> GetAllAsync(AuctionQueryParameters query, CancellationToken cancellationToken);
    Task<AuctionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AuctionDto> CreateAsync(Guid producerId, CreateAuctionRequest request, CancellationToken cancellationToken);
    Task<AuctionDto> PlaceBidAsync(Guid id, Guid bidderId, PlaceBidRequest request, CancellationToken cancellationToken);
    Task<AuctionDto> CancelAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
}
