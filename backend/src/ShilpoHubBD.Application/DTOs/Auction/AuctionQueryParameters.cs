using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Application.DTOs.Auction;

public class AuctionQueryParameters
{
    public AuctionStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
