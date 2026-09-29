using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Auction;
using ShilpoHubBD.Domain.Entities.Auction;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using AuctionEntity = ShilpoHubBD.Domain.Entities.Auction.Auction;

namespace ShilpoHubBD.UnitTests.Features.Auction.Services;

[Trait("Feature", "Auction")]
[Trait("Layer", "Service")]
public class AuctionServiceTests
{
    private readonly IAuctionRepository _auctions = Substitute.For<IAuctionRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly Guid _producerId = Guid.NewGuid();

    public AuctionServiceTests()
    {
        _auctions.GetDueForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(new List<AuctionEntity>());
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuctionService CreateService() => new(_auctions, _products);

    private AuctionEntity MakeAuction(
        AuctionStatus status = AuctionStatus.Active, decimal currentPrice = 1000, decimal minBidIncrement = 100,
        DateTime? startAt = null, DateTime? endAt = null, Guid? producerId = null)
    {
        var auction = new AuctionEntity
        {
            Id = Guid.NewGuid(), ProducerId = producerId ?? _producerId, Status = status, CurrentPrice = currentPrice,
            StartingPrice = currentPrice, MinBidIncrement = minBidIncrement,
            StartAt = startAt ?? DateTime.UtcNow.AddHours(-1), EndAt = endAt ?? DateTime.UtcNow.AddDays(1),
            Producer = TestUsers.Create(fullName: "Rahima Begum"),
            Product = new Product { Id = Guid.NewGuid(), Name = "Jamdani Saree" },
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _auctions.GetByIdAsync(auction.Id, Arg.Any<CancellationToken>()).Returns(auction);
        return auction;
    }

    private Product MakeProduct()
    {
        var product = new Product { Id = Guid.NewGuid(), ProducerId = _producerId, Name = "Jamdani Saree" };
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        return product;
    }

    // ---------- GetMineAsync / GetAllAsync ----------

    [Theory]
    [InlineData(0, 20, 1, 20)]
    [InlineData(-1, 20, 1, 20)]
    [InlineData(2, 0, 2, 1)]
    [InlineData(2, 51, 2, 50)]
    public async Task GetMineAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _auctions.GetPagedForProducerAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<AuctionEntity>(), 0));

        var result = await CreateService().GetMineAsync(_producerId, page, pageSize, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
    }

    [Fact]
    public async Task GetMineAsync_SyncsDueAuctionsFirst()
    {
        _auctions.GetPagedForProducerAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<AuctionEntity>(), 0));

        await CreateService().GetMineAsync(_producerId, 1, 10, Ct);

        await _auctions.Received(1).GetDueForSyncAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMineAsync_MapsEachAuctionAndTotalCount()
    {
        var auction = MakeAuction();
        _auctions.GetPagedForProducerAsync(_producerId, 1, 10, Arg.Any<CancellationToken>()).Returns((new List<AuctionEntity> { auction }, 7));

        var result = await CreateService().GetMineAsync(_producerId, 1, 10, Ct);

        Assert.Equal(7, result.TotalCount);
        Assert.Equal(auction.Id, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task GetAllAsync_PassesTheStatusFilterAndPagingThrough()
    {
        _auctions.GetPagedAsync(AuctionStatus.Active, 2, 10, Arg.Any<CancellationToken>()).Returns((new List<AuctionEntity>(), 0));

        await CreateService().GetAllAsync(new AuctionQueryParameters { Status = AuctionStatus.Active, Page = 2, PageSize = 10 }, Ct);

        await _auctions.Received(1).GetPagedAsync(AuctionStatus.Active, 2, 10, Arg.Any<CancellationToken>());
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingAuction_ReturnsIt()
    {
        var auction = MakeAuction();

        var dto = await CreateService().GetByIdAsync(auction.Id, Ct);

        Assert.Equal(auction.Id, dto.Id);
        Assert.Equal("Rahima Begum", dto.ProducerName);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownAuction_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Auction not found.", error.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ScheduledAuctionPastItsStartTime_IsAdvancedToActiveAndSaved()
    {
        var auction = MakeAuction(AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddMinutes(-1), endAt: DateTime.UtcNow.AddDays(1));

        var dto = await CreateService().GetByIdAsync(auction.Id, Ct);

        Assert.Equal("Active", dto.Status);
        await _auctions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ActiveAuctionPastItsEndTime_EndsAndRecordsTheHighestBidderAsWinner()
    {
        var auction = MakeAuction(AuctionStatus.Active, endAt: DateTime.UtcNow.AddMinutes(-1));
        var lowBidder = TestUsers.Create();
        var highBidder = TestUsers.Create();
        auction.Bids.Add(new AuctionBid { Id = Guid.NewGuid(), BidderId = lowBidder.Id, Bidder = lowBidder, Amount = 1100, CreatedAt = DateTime.UtcNow });
        auction.Bids.Add(new AuctionBid { Id = Guid.NewGuid(), BidderId = highBidder.Id, Bidder = highBidder, Amount = 1500, CreatedAt = DateTime.UtcNow });

        var dto = await CreateService().GetByIdAsync(auction.Id, Ct);

        Assert.Equal("Ended", dto.Status);
        Assert.Equal(highBidder.Id, dto.WinnerId);
        Assert.Equal(0, dto.TimeRemainingSeconds);
    }

    [Fact]
    public async Task GetByIdAsync_EndsWithNoBids_HasNoWinner()
    {
        var auction = MakeAuction(AuctionStatus.Active, endAt: DateTime.UtcNow.AddMinutes(-1));

        var dto = await CreateService().GetByIdAsync(auction.Id, Ct);

        Assert.Equal("Ended", dto.Status);
        Assert.Null(dto.WinnerId);
    }

    [Fact]
    public async Task GetByIdAsync_TiedTopBids_PicksTheEarlierOne()
    {
        var auction = MakeAuction(AuctionStatus.Active, endAt: DateTime.UtcNow.AddMinutes(-1));
        var earlier = TestUsers.Create();
        var later = TestUsers.Create();
        auction.Bids.Add(new AuctionBid { Id = Guid.NewGuid(), BidderId = later.Id, Bidder = later, Amount = 1500, CreatedAt = DateTime.UtcNow.AddMinutes(-5) });
        auction.Bids.Add(new AuctionBid { Id = Guid.NewGuid(), BidderId = earlier.Id, Bidder = earlier, Amount = 1500, CreatedAt = DateTime.UtcNow.AddMinutes(-10) });

        var dto = await CreateService().GetByIdAsync(auction.Id, Ct);

        Assert.Equal(earlier.Id, dto.WinnerId);
    }

    [Fact]
    public async Task GetByIdAsync_StatusAlreadyUpToDate_DoesNotSave()
    {
        var auction = MakeAuction(AuctionStatus.Active);

        await CreateService().GetByIdAsync(auction.Id, Ct);

        await _auctions.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAScheduledAuctionWithCurrentPriceEqualToStartingPrice()
    {
        var product = MakeProduct();
        AuctionEntity? saved = null;
        // CreateAsync only sets ProducerId/ProductId on the new entity; the real repository's
        // GetByIdAsync loads Producer/Product via Include when re-fetching it afterwards, so the fake
        // does the same here.
        await _auctions.AddAsync(Arg.Do<AuctionEntity>(a =>
        {
            a.Producer = TestUsers.Create(fullName: "Rahima Begum");
            a.Product = product;
            saved = a;
        }), Arg.Any<CancellationToken>());
        _auctions.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => saved);
        var request = new CreateAuctionRequest
        {
            ProductId = product.Id, Title = "  Antique Saree  ", Description = "  Rare  ", StartingPrice = 1000,
            MinBidIncrement = 50, StartAt = DateTime.UtcNow.AddHours(1), EndAt = DateTime.UtcNow.AddDays(7),
        };
        var before = DateTime.UtcNow;

        await CreateService().CreateAsync(_producerId, request, Ct);

        Assert.NotNull(saved);
        Assert.Equal(_producerId, saved.ProducerId);
        Assert.Equal(product.Id, saved.ProductId);
        Assert.Equal("Antique Saree", saved.Title);
        Assert.Equal("Rare", saved.Description);
        Assert.Equal(1000, saved.StartingPrice);
        Assert.Equal(1000, saved.CurrentPrice);
        Assert.Equal(AuctionStatus.Scheduled, saved.Status);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _auctions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().CreateAsync(_producerId, new CreateAuctionRequest { ProductId = Guid.NewGuid() }, Ct));

        Assert.Equal("Product not found.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_AnotherProducersProduct_ThrowsUnauthorizedAndSavesNothing()
    {
        var product = MakeProduct();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().CreateAsync(Guid.NewGuid(), new CreateAuctionRequest { ProductId = product.Id }, Ct));

        Assert.Equal("You can only auction your own products.", error.Message);
        await _auctions.DidNotReceive().AddAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>());
    }

    // ---------- PlaceBidAsync ----------

    [Fact]
    public async Task PlaceBidAsync_ValidBidAboveMinimum_RecordsItAndUpdatesCurrentPrice()
    {
        var auction = MakeAuction(currentPrice: 1000, minBidIncrement: 100);
        var bidder = Guid.NewGuid();
        AuctionBid? saved = null;
        await _auctions.AddBidAsync(Arg.Do<AuctionBid>(b => saved = b), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().PlaceBidAsync(auction.Id, bidder, new PlaceBidRequest { Amount = 1200 }, Ct);

        Assert.NotNull(saved);
        Assert.Equal(auction.Id, saved.AuctionId);
        Assert.Equal(bidder, saved.BidderId);
        Assert.Equal(1200, saved.Amount);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(1200, auction.CurrentPrice);
        await _auctions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(1200, dto.CurrentPrice);
    }

    [Fact]
    public async Task PlaceBidAsync_ExactlyTheMinimumAcceptableAmount_Succeeds()
    {
        var auction = MakeAuction(currentPrice: 1000, minBidIncrement: 100);

        var dto = await CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 1100 }, Ct);

        Assert.Equal(1100, dto.CurrentPrice);
    }

    [Fact]
    public async Task PlaceBidAsync_BelowTheMinimumAcceptableAmount_ThrowsConflictNamingTheMinimum()
    {
        var auction = MakeAuction(currentPrice: 1000, minBidIncrement: 100);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 1050 }, Ct));

        Assert.Equal("Your bid must be at least 1100.", error.Message);
        Assert.Equal(1000, auction.CurrentPrice);
        await _auctions.DidNotReceive().AddBidAsync(Arg.Any<AuctionBid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlaceBidAsync_ProducerBiddingOnTheirOwnAuction_ThrowsConflict()
    {
        var auction = MakeAuction();

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().PlaceBidAsync(auction.Id, _producerId, new PlaceBidRequest { Amount = 2000 }, Ct));

        Assert.Equal("You cannot bid on your own auction.", error.Message);
    }

    [Fact]
    public async Task PlaceBidAsync_AuctionNotYetStarted_ThrowsConflict()
    {
        var auction = MakeAuction(AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddHours(1), endAt: DateTime.UtcNow.AddDays(1));

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 2000 }, Ct));

        Assert.Equal("This auction has not started yet.", error.Message);
    }

    [Fact]
    public async Task PlaceBidAsync_AuctionAlreadyEnded_ThrowsConflict()
    {
        var auction = MakeAuction(AuctionStatus.Ended);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 2000 }, Ct));

        Assert.Equal("This auction has already ended.", error.Message);
    }

    [Fact]
    public async Task PlaceBidAsync_AuctionCancelled_ThrowsConflict()
    {
        var auction = MakeAuction(AuctionStatus.Cancelled);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 2000 }, Ct));

        Assert.Equal("This auction has been cancelled.", error.Message);
    }

    [Fact]
    public async Task PlaceBidAsync_ScheduledAuctionPastItsStartTime_IsSyncedToActiveAndAcceptsTheBid()
    {
        var auction = MakeAuction(AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddMinutes(-1), endAt: DateTime.UtcNow.AddDays(1));

        var dto = await CreateService().PlaceBidAsync(auction.Id, Guid.NewGuid(), new PlaceBidRequest { Amount = 1500 }, Ct);

        Assert.Equal("Active", dto.Status);
    }

    [Fact]
    public async Task PlaceBidAsync_UnknownAuction_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().PlaceBidAsync(Guid.NewGuid(), Guid.NewGuid(), new PlaceBidRequest { Amount = 1 }, Ct));

        Assert.Equal("Auction not found.", error.Message);
    }

    // ---------- CancelAsync ----------

    [Fact]
    public async Task CancelAsync_OwningProducer_CancelsTheAuction()
    {
        var auction = MakeAuction(AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddHours(1), endAt: DateTime.UtcNow.AddDays(1));
        var before = DateTime.UtcNow;

        var dto = await CreateService().CancelAsync(auction.Id, _producerId, false, Ct);

        Assert.Equal(AuctionStatus.Cancelled, auction.Status);
        Assert.InRange(auction.UpdatedAt, before, DateTime.UtcNow);
        Assert.Equal("Cancelled", dto.Status);
        await _auctions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_Admin_CanCancelAnyProducersAuction()
    {
        var auction = MakeAuction(AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddHours(1), endAt: DateTime.UtcNow.AddDays(1));

        var dto = await CreateService().CancelAsync(auction.Id, Guid.NewGuid(), true, Ct);

        Assert.Equal("Cancelled", dto.Status);
    }

    [Fact]
    public async Task CancelAsync_AnotherProducersAuctionWithoutAdmin_ThrowsUnauthorized()
    {
        var auction = MakeAuction();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().CancelAsync(auction.Id, Guid.NewGuid(), false, Ct));

        Assert.Equal("You do not have permission to manage this auction.", error.Message);
    }

    [Fact]
    public async Task CancelAsync_AlreadyEnded_ThrowsConflictAndSavesNothing()
    {
        var auction = MakeAuction(AuctionStatus.Active, endAt: DateTime.UtcNow.AddMinutes(-1));

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().CancelAsync(auction.Id, _producerId, false, Ct));

        Assert.Equal("This auction has already ended.", error.Message);
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ThrowsConflict()
    {
        var auction = MakeAuction(AuctionStatus.Cancelled);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().CancelAsync(auction.Id, _producerId, false, Ct));

        Assert.Equal("This auction is already cancelled.", error.Message);
    }

    [Fact]
    public async Task CancelAsync_UnknownAuction_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CancelAsync(Guid.NewGuid(), _producerId, false, Ct));

        Assert.Equal("Auction not found.", error.Message);
    }
}
