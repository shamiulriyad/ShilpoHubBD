using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Auction;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;
using AuctionEntity = ShilpoHubBD.Domain.Entities.Auction.Auction;

namespace ShilpoHubBD.UnitTests.Features.Auction.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Auction")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class AuctionRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public AuctionRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(User Producer, Product Product)> SeedProductAsync(ShilpoHubDbContext context)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var district = await context.Districts.FirstAsync(Ct);
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Jamdani Saree", Slug = Guid.NewGuid().ToString("N"), Price = 1000, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return (producer, product);
    }

    private static AuctionEntity MakeAuction(
        User producer, Product product, AuctionStatus status = AuctionStatus.Active, DateTime? startAt = null, DateTime? endAt = null, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(), ProducerId = producer.Id, ProductId = product.Id, Title = "Antique Saree", Description = "x",
        StartingPrice = 1000, CurrentPrice = 1000, MinBidIncrement = 50, Status = status,
        StartAt = startAt ?? DateTime.UtcNow.AddHours(-1), EndAt = endAt ?? DateTime.UtcNow.AddDays(1),
        CreatedAt = createdAt ?? DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheAuction()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var auction = MakeAuction(producer, product);
        var repository = new AuctionRepository(db.NewContext());

        await repository.AddAsync(auction, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().Auctions.AnyAsync(a => a.Id == auction.Id, Ct));
    }

    [Fact]
    public async Task GetByIdAsync_LoadsProducerProductWinnerAndBidsWithBidders()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var bidder = TestUsers.Create();
        context.Users.Add(bidder);
        var auction = MakeAuction(producer, product, AuctionStatus.Ended);
        auction.WinnerId = bidder.Id;
        context.Auctions.Add(auction);
        context.AuctionBids.Add(new AuctionBid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = bidder.Id, Amount = 1200, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(Ct);

        var found = await new AuctionRepository(db.NewContext()).GetByIdAsync(auction.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(producer.Id, found.Producer.Id);
        Assert.Equal(product.Id, found.Product.Id);
        Assert.Equal(bidder.Id, found.Winner!.Id);
        Assert.Equal(bidder.Id, Assert.Single(found.Bids).Bidder.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new AuctionRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task AddBidAsync_ThenSaveChangesAsync_PersistsTheBid()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var bidder = TestUsers.Create();
        context.Users.Add(bidder);
        var auction = MakeAuction(producer, product);
        context.Auctions.Add(auction);
        await context.SaveChangesAsync(Ct);
        var bid = new AuctionBid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = bidder.Id, Amount = 1100, CreatedAt = DateTime.UtcNow };
        var repository = new AuctionRepository(db.NewContext());

        await repository.AddBidAsync(bid, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().AuctionBids.AnyAsync(b => b.Id == bid.Id, Ct));
    }

    [Fact]
    public async Task GetPagedAsync_ByStatus_ReturnsOnlyThatStatus()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var active = MakeAuction(producer, product, AuctionStatus.Active);
        var (producer2, product2) = await SeedProductAsync(context);
        var scheduled = MakeAuction(producer2, product2, AuctionStatus.Scheduled);
        context.Auctions.AddRange(active, scheduled);
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new AuctionRepository(db.NewContext()).GetPagedAsync(AuctionStatus.Active, 1, 20, Ct);

        Assert.Contains(items, a => a.Id == active.Id);
        Assert.DoesNotContain(items, a => a.Id == scheduled.Id);
        Assert.True(total >= 1);
    }

    [Fact]
    public async Task GetPagedAsync_NoStatusFilter_OrdersByEndAtAscending()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var (producer2, product2) = await SeedProductAsync(context);
        var soon = MakeAuction(producer, product, endAt: DateTime.UtcNow.AddHours(2));
        var later = MakeAuction(producer2, product2, endAt: DateTime.UtcNow.AddDays(10));
        context.Auctions.AddRange(later, soon);
        await context.SaveChangesAsync(Ct);

        var (items, _) = await new AuctionRepository(db.NewContext()).GetPagedAsync(null, 1, 100, Ct);

        var soonIndex = items.FindIndex(a => a.Id == soon.Id);
        var laterIndex = items.FindIndex(a => a.Id == later.Id);
        Assert.True(soonIndex < laterIndex);
    }

    [Fact]
    public async Task GetDueForSyncAsync_ScheduledAuctionPastItsStartTime_IsIncluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var due = MakeAuction(producer, product, AuctionStatus.Scheduled, startAt: DateTime.UtcNow.AddMinutes(-5), endAt: DateTime.UtcNow.AddDays(1));
        context.Auctions.Add(due);
        await context.SaveChangesAsync(Ct);

        var results = await new AuctionRepository(db.NewContext()).GetDueForSyncAsync(DateTime.UtcNow, Ct);

        Assert.Contains(results, a => a.Id == due.Id);
    }

    [Fact]
    public async Task GetDueForSyncAsync_ActiveAuctionPastItsEndTime_IsIncluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var due = MakeAuction(producer, product, AuctionStatus.Active, endAt: DateTime.UtcNow.AddMinutes(-1));
        context.Auctions.Add(due);
        await context.SaveChangesAsync(Ct);

        var results = await new AuctionRepository(db.NewContext()).GetDueForSyncAsync(DateTime.UtcNow, Ct);

        Assert.Contains(results, a => a.Id == due.Id);
    }

    [Fact]
    public async Task GetDueForSyncAsync_AuctionStillWithinItsWindow_IsNotIncluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var notDue = MakeAuction(producer, product, AuctionStatus.Active, startAt: DateTime.UtcNow.AddHours(-1), endAt: DateTime.UtcNow.AddDays(1));
        context.Auctions.Add(notDue);
        await context.SaveChangesAsync(Ct);

        var results = await new AuctionRepository(db.NewContext()).GetDueForSyncAsync(DateTime.UtcNow, Ct);

        Assert.DoesNotContain(results, a => a.Id == notDue.Id);
    }

    [Fact]
    public async Task GetPagedForProducerAsync_ReturnsOnlyThatProducersAuctionsNewestFirst()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (producer, product) = await SeedProductAsync(context);
        var older = MakeAuction(producer, product, createdAt: DateTime.UtcNow.AddDays(-1));
        var newer = MakeAuction(producer, product, createdAt: DateTime.UtcNow);
        var (otherProducer, otherProduct) = await SeedProductAsync(context);
        var othersAuction = MakeAuction(otherProducer, otherProduct);
        context.Auctions.AddRange(older, newer, othersAuction);
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new AuctionRepository(db.NewContext()).GetPagedForProducerAsync(producer.Id, 1, 20, Ct);

        Assert.Equal(2, total);
        Assert.Equal(new[] { newer.Id, older.Id }, items.Select(a => a.Id));
    }
}
