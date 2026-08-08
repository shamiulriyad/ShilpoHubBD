using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveShopping;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Application.Services.LiveShopping;

public class LiveShoppingService : ILiveShoppingService
{
    private readonly ILiveShoppingRepository _liveShoppingRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICartService _cartService;
    private readonly ILiveEventNotifier _liveEventNotifier;

    public LiveShoppingService(
        ILiveShoppingRepository liveShoppingRepository,
        IProductRepository productRepository,
        IAuctionRepository auctionRepository,
        ICartService cartService,
        ILiveEventNotifier liveEventNotifier)
    {
        _liveShoppingRepository = liveShoppingRepository;
        _productRepository = productRepository;
        _auctionRepository = auctionRepository;
        _cartService = cartService;
        _liveEventNotifier = liveEventNotifier;
    }

    public async Task<PagedResult<LiveEventListItemDto>> GetAllAsync(LiveEventQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _liveShoppingRepository.GetPagedAsync(query.Status, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<LiveEventListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<LiveEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        return ToDto(liveEvent);
    }

    public async Task<LiveEventDto> CreateAsync(Guid producerId, CreateLiveEventRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You can only host live events for your own products.");
        }

        if (request.AuctionId.HasValue)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId.Value, cancellationToken)
                ?? throw new NotFoundException("Auction not found.");

            if (auction.ProducerId != producerId)
            {
                throw new UnauthorizedAccessException("You can only link auctions you own to a live event.");
            }
        }

        var now = DateTime.UtcNow;
        var liveEvent = new LiveEvent
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            ProductId = request.ProductId,
            AuctionId = request.AuctionId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = LiveEventStatus.Scheduled,
            ScheduledStartAt = request.ScheduledStartAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _liveShoppingRepository.AddAsync(liveEvent, cancellationToken);
        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        var created = await _liveShoppingRepository.GetByIdAsync(liveEvent.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<LiveEventDto> StartAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        EnsureOwner(liveEvent, producerId, isAdmin);

        if (liveEvent.Status != LiveEventStatus.Scheduled)
        {
            throw new ConflictException("Only scheduled live events can be started.");
        }

        var now = DateTime.UtcNow;
        liveEvent.Status = LiveEventStatus.Live;
        liveEvent.StartedAt = now;
        liveEvent.UpdatedAt = now;

        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        var dto = ToDto(liveEvent);
        await _liveEventNotifier.NotifyStatusChangedAsync(id, dto, cancellationToken);
        return dto;
    }

    public async Task<LiveEventDto> EndAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        EnsureOwner(liveEvent, producerId, isAdmin);

        if (liveEvent.Status != LiveEventStatus.Live)
        {
            throw new ConflictException("Only a live event that is currently live can be ended.");
        }

        var now = DateTime.UtcNow;
        liveEvent.Status = LiveEventStatus.Ended;
        liveEvent.EndedAt = now;
        liveEvent.UpdatedAt = now;

        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        var dto = ToDto(liveEvent);
        await _liveEventNotifier.NotifyStatusChangedAsync(id, dto, cancellationToken);
        return dto;
    }

    public async Task<LiveEventCommentDto> AddCommentAsync(Guid id, Guid userId, AddLiveCommentRequest request, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        if (liveEvent.Status != LiveEventStatus.Live)
        {
            throw new ConflictException("You can only comment while the event is live.");
        }

        var comment = new LiveEventComment
        {
            Id = Guid.NewGuid(),
            LiveEventId = id,
            UserId = userId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _liveShoppingRepository.AddCommentAsync(comment, cancellationToken);
        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        var updated = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken);
        var saved = updated!.Comments.First(c => c.Id == comment.Id);
        var dto = ToCommentDto(saved);

        await _liveEventNotifier.NotifyCommentAsync(id, dto, cancellationToken);
        return dto;
    }

    public async Task<List<ReactionSummaryDto>> AddReactionAsync(Guid id, Guid userId, AddLiveReactionRequest request, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        if (liveEvent.Status != LiveEventStatus.Live)
        {
            throw new ConflictException("You can only react while the event is live.");
        }

        var reaction = new LiveEventReaction
        {
            Id = Guid.NewGuid(),
            LiveEventId = id,
            UserId = userId,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
        };

        await _liveShoppingRepository.AddReactionAsync(reaction, cancellationToken);
        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        var updated = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken);
        var summary = ToReactionSummary(updated!);

        await _liveEventNotifier.NotifyReactionAsync(id, summary, cancellationToken);
        return summary;
    }

    public async Task<CartItemDto> BuyDuringLiveAsync(Guid id, Guid userId, BuyDuringLiveRequest request, CancellationToken cancellationToken)
    {
        var liveEvent = await _liveShoppingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Live event not found.");

        if (liveEvent.Status != LiveEventStatus.Live)
        {
            throw new ConflictException("Purchases are only available while the event is live.");
        }

        var cartItem = await _cartService.AddOrIncrementAsync(userId, liveEvent.ProductId, request.ProductVariantId, request.Quantity, cancellationToken);

        var purchase = new LiveEventPurchase
        {
            Id = Guid.NewGuid(),
            LiveEventId = id,
            UserId = userId,
            ProductId = liveEvent.ProductId,
            Quantity = request.Quantity,
            UnitPrice = cartItem.UnitPrice,
            CreatedAt = DateTime.UtcNow,
        };

        await _liveShoppingRepository.AddPurchaseAsync(purchase, cancellationToken);
        await _liveShoppingRepository.SaveChangesAsync(cancellationToken);

        await _liveEventNotifier.NotifyPurchaseAsync(id, liveEvent.Purchases.Count, cancellationToken);
        return cartItem;
    }

    private static void EnsureOwner(LiveEvent liveEvent, Guid userId, bool isAdmin)
    {
        if (!isAdmin && liveEvent.ProducerId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this live event.");
        }
    }

    private static LiveEventListItemDto ToListItemDto(LiveEvent liveEvent) => new()
    {
        Id = liveEvent.Id,
        ProducerName = liveEvent.Producer.FullName,
        ProductId = liveEvent.ProductId,
        ProductName = liveEvent.Product.Name,
        ProductImageUrl = liveEvent.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        HasLiveAuction = liveEvent.AuctionId.HasValue,
        Title = liveEvent.Title,
        Status = liveEvent.Status.ToString(),
        ScheduledStartAt = liveEvent.ScheduledStartAt,
        StartedAt = liveEvent.StartedAt,
        ReactionCount = liveEvent.Reactions.Count,
    };

    private static LiveEventDto ToDto(LiveEvent liveEvent) => new()
    {
        Id = liveEvent.Id,
        ProducerId = liveEvent.ProducerId,
        ProducerName = liveEvent.Producer.FullName,
        ProductId = liveEvent.ProductId,
        ProductName = liveEvent.Product.Name,
        ProductImageUrl = liveEvent.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        ProductPrice = liveEvent.Product.DiscountPrice ?? liveEvent.Product.Price,
        AuctionId = liveEvent.AuctionId,
        AuctionTitle = liveEvent.Auction?.Title,
        AuctionStatus = liveEvent.Auction?.Status.ToString(),
        AuctionCurrentPrice = liveEvent.Auction?.CurrentPrice,
        Title = liveEvent.Title,
        Description = liveEvent.Description,
        Status = liveEvent.Status.ToString(),
        ScheduledStartAt = liveEvent.ScheduledStartAt,
        StartedAt = liveEvent.StartedAt,
        EndedAt = liveEvent.EndedAt,
        CommentCount = liveEvent.Comments.Count,
        PurchaseCount = liveEvent.Purchases.Count,
        ReactionSummary = ToReactionSummary(liveEvent),
        Comments = liveEvent.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(ToCommentDto)
            .ToList(),
        CreatedAt = liveEvent.CreatedAt,
        UpdatedAt = liveEvent.UpdatedAt,
    };

    private static List<ReactionSummaryDto> ToReactionSummary(LiveEvent liveEvent)
        => liveEvent.Reactions
            .GroupBy(r => r.Type)
            .Select(g => new ReactionSummaryDto { Type = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(r => r.Count)
            .ToList();

    private static LiveEventCommentDto ToCommentDto(LiveEventComment comment) => new()
    {
        Id = comment.Id,
        UserId = comment.UserId,
        AuthorName = comment.User.FullName,
        Body = comment.Body,
        CreatedAt = comment.CreatedAt,
    };
}
