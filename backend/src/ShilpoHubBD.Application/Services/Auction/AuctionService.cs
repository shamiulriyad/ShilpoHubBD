using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.Application.Services.Auction;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IProductRepository _productRepository;

    public AuctionService(IAuctionRepository auctionRepository, IProductRepository productRepository)
    {
        _auctionRepository = auctionRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResult<AuctionListItemDto>> GetAllAsync(AuctionQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _auctionRepository.GetPagedAsync(query.Status, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<AuctionListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<AuctionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Auction not found.");

        if (SyncStatus(auction))
        {
            await _auctionRepository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(auction);
    }

    public async Task<AuctionDto> CreateAsync(Guid producerId, CreateAuctionRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You can only auction your own products.");
        }

        var now = DateTime.UtcNow;
        var auction = new Domain.Entities.Auction.Auction
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            ProductId = request.ProductId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            StartingPrice = request.StartingPrice,
            CurrentPrice = request.StartingPrice,
            MinBidIncrement = request.MinBidIncrement,
            Status = AuctionStatus.Scheduled,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _auctionRepository.AddAsync(auction, cancellationToken);
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        var created = await _auctionRepository.GetByIdAsync(auction.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<AuctionDto> PlaceBidAsync(Guid id, Guid bidderId, PlaceBidRequest request, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Auction not found.");

        SyncStatus(auction);

        if (auction.ProducerId == bidderId)
        {
            throw new ConflictException("You cannot bid on your own auction.");
        }

        switch (auction.Status)
        {
            case AuctionStatus.Scheduled:
                throw new ConflictException("This auction has not started yet.");
            case AuctionStatus.Ended:
                throw new ConflictException("This auction has already ended.");
            case AuctionStatus.Cancelled:
                throw new ConflictException("This auction has been cancelled.");
        }

        var minimumAmount = auction.CurrentPrice + auction.MinBidIncrement;
        if (request.Amount < minimumAmount)
        {
            throw new ConflictException($"Your bid must be at least {minimumAmount}.");
        }

        var now = DateTime.UtcNow;
        var bid = new AuctionBid
        {
            Id = Guid.NewGuid(),
            AuctionId = id,
            BidderId = bidderId,
            Amount = request.Amount,
            CreatedAt = now,
        };

        await _auctionRepository.AddBidAsync(bid, cancellationToken);
        auction.CurrentPrice = request.Amount;
        auction.UpdatedAt = now;
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        var updated = await _auctionRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task<AuctionDto> CancelAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Auction not found.");

        if (!isAdmin && auction.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this auction.");
        }

        SyncStatus(auction);

        if (auction.Status == AuctionStatus.Ended)
        {
            throw new ConflictException("This auction has already ended.");
        }

        if (auction.Status == AuctionStatus.Cancelled)
        {
            throw new ConflictException("This auction is already cancelled.");
        }

        auction.Status = AuctionStatus.Cancelled;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        return ToDto(auction);
    }

    /// <summary>
    /// Advances a Scheduled auction to Active once its start time has passed, and finalizes an
    /// Active/Scheduled auction to Ended (recording the highest bidder as winner) once its end
    /// time has passed. Returns true if the auction's persisted state changed.
    /// </summary>
    private static bool SyncStatus(Domain.Entities.Auction.Auction auction)
    {
        var now = DateTime.UtcNow;
        var changed = false;

        if (auction.Status == AuctionStatus.Scheduled && now >= auction.StartAt && now < auction.EndAt)
        {
            auction.Status = AuctionStatus.Active;
            changed = true;
        }

        if ((auction.Status == AuctionStatus.Scheduled || auction.Status == AuctionStatus.Active) && now >= auction.EndAt)
        {
            var topBid = auction.Bids.OrderByDescending(b => b.Amount).ThenBy(b => b.CreatedAt).FirstOrDefault();
            auction.Status = AuctionStatus.Ended;
            auction.WinnerId = topBid?.BidderId;
            auction.Winner = topBid?.Bidder;
            changed = true;
        }

        return changed;
    }

    private static long TimeRemainingSeconds(Domain.Entities.Auction.Auction auction)
    {
        var now = DateTime.UtcNow;
        if (auction.Status != AuctionStatus.Active || now >= auction.EndAt)
        {
            return 0;
        }

        return (long)(auction.EndAt - now).TotalSeconds;
    }

    private static AuctionListItemDto ToListItemDto(Domain.Entities.Auction.Auction auction) => new()
    {
        Id = auction.Id,
        ProducerName = auction.Producer.FullName,
        ProductId = auction.ProductId,
        ProductName = auction.Product.Name,
        ProductImageUrl = auction.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        Title = auction.Title,
        CurrentPrice = auction.CurrentPrice,
        Status = auction.Status.ToString(),
        StartAt = auction.StartAt,
        EndAt = auction.EndAt,
        TimeRemainingSeconds = TimeRemainingSeconds(auction),
        BidCount = auction.Bids.Count,
    };

    private static AuctionDto ToDto(Domain.Entities.Auction.Auction auction) => new()
    {
        Id = auction.Id,
        ProducerId = auction.ProducerId,
        ProducerName = auction.Producer.FullName,
        ProductId = auction.ProductId,
        ProductName = auction.Product.Name,
        ProductImageUrl = auction.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        Title = auction.Title,
        Description = auction.Description,
        StartingPrice = auction.StartingPrice,
        CurrentPrice = auction.CurrentPrice,
        MinBidIncrement = auction.MinBidIncrement,
        Status = auction.Status.ToString(),
        StartAt = auction.StartAt,
        EndAt = auction.EndAt,
        TimeRemainingSeconds = TimeRemainingSeconds(auction),
        WinnerId = auction.WinnerId,
        WinnerName = auction.Winner?.FullName,
        BidCount = auction.Bids.Count,
        Bids = auction.Bids
            .OrderByDescending(b => b.Amount)
            .ThenBy(b => b.CreatedAt)
            .Select(b => new AuctionBidDto
            {
                Id = b.Id,
                BidderId = b.BidderId,
                BidderName = b.Bidder.FullName,
                Amount = b.Amount,
                CreatedAt = b.CreatedAt,
            })
            .ToList(),
        CreatedAt = auction.CreatedAt,
        UpdatedAt = auction.UpdatedAt,
    };
}
