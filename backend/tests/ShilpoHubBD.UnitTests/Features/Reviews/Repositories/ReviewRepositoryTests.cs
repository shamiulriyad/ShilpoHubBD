using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Domain.Entities.TouristBooking;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Reviews")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class ReviewRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ReviewRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<Product> SeedProductAsync(ShilpoHubDbContext context)
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
        return product;
    }

    private static async Task<HeritagePlace> SeedHeritagePlaceAsync(ShilpoHubDbContext context)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var place = new HeritagePlace
        {
            Id = Guid.NewGuid(), Name = "Test Place " + Guid.NewGuid().ToString("N")[..8], Description = "x",
            PlaceType = HeritagePlaceType.Museum, Latitude = 23.7, Longitude = 90.4, DistrictId = district.Id,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.HeritagePlaces.Add(place);
        await context.SaveChangesAsync(Ct);
        return place;
    }

    /// <summary>Seeds a full booking chain (producer, District, TouristService, ServiceAvailabilitySlot,
    /// tourist) since Booking has required foreign keys to all of them.</summary>
    private static async Task<Booking> SeedBookingAsync(ShilpoHubDbContext context, User tourist, BookingStatus status = BookingStatus.Completed)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var producer = TestUsers.Create();
        var service = new TouristService
        {
            Id = Guid.NewGuid(), Title = "Heritage Walk", Description = "x", Price = 500,
            ProducerId = producer.Id, DistrictId = district.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var slot = new ServiceAvailabilitySlot
        {
            Id = Guid.NewGuid(), ServiceId = service.Id, StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var booking = new Booking
        {
            Id = Guid.NewGuid(), ServiceId = service.Id, AvailabilitySlotId = slot.Id, TouristId = tourist.Id, ProducerId = producer.Id,
            Status = status, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Users.Add(producer);
        context.TouristServices.Add(service);
        context.ServiceAvailabilitySlots.Add(slot);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(Ct);
        return booking;
    }

    private static Review MakeReview(User reviewer, Guid? productId = null, Guid? placeId = null, Guid? bookingId = null, int rating = 5, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(), UserId = reviewer.Id, ProductId = productId, HeritagePlaceId = placeId, BookingId = bookingId,
        Rating = rating, Comment = "x", CreatedAt = createdAt ?? DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheReview()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        await context.SaveChangesAsync(Ct);
        var review = MakeReview(reviewer, productId: product.Id);
        var repository = new ReviewRepository(db.NewContext());

        await repository.AddAsync(review, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().Reviews.AnyAsync(r => r.Id == review.Id, Ct));
    }

    [Fact]
    public async Task GetPagedByProductAsync_ReturnsNewestFirstWithReviewerAndImagesLoaded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        // One review per user per product is enforced, so "older" and "newer" need different reviewers.
        var olderReviewer = TestUsers.Create();
        var newerReviewer = TestUsers.Create(fullName: "Rahima Begum");
        context.Users.AddRange(olderReviewer, newerReviewer);
        var older = MakeReview(olderReviewer, productId: product.Id, createdAt: DateTime.UtcNow.AddDays(-1));
        var newer = MakeReview(newerReviewer, productId: product.Id, createdAt: DateTime.UtcNow);
        context.Reviews.AddRange(older, newer);
        context.ReviewImages.Add(new ReviewImage { Id = Guid.NewGuid(), ReviewId = newer.Id, ImageUrl = "a.jpg", DisplayOrder = 0 });
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new ReviewRepository(db.NewContext()).GetPagedByProductAsync(product.Id, 1, 20, Ct);

        Assert.Equal(2, total);
        Assert.Equal(new[] { newer.Id, older.Id }, items.Select(r => r.Id));
        Assert.Equal("Rahima Begum", items[0].User.FullName);
        Assert.Single(items[0].Images);
    }

    [Fact]
    public async Task GetPagedByHeritagePlaceAsync_ReturnsOnlyThatPlacesReviews()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var place = await SeedHeritagePlaceAsync(context);
        var otherPlace = await SeedHeritagePlaceAsync(context);
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        var review = MakeReview(reviewer, placeId: place.Id);
        var otherReview = MakeReview(reviewer, placeId: otherPlace.Id);
        context.Reviews.AddRange(review, otherReview);
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new ReviewRepository(db.NewContext()).GetPagedByHeritagePlaceAsync(place.Id, 1, 20, Ct);

        Assert.Equal(1, total);
        Assert.Equal(review.Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetPagedByServiceAsync_ReturnsOnlyReviewsOfBookingsForThatService()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        await context.SaveChangesAsync(Ct);
        var booking = await SeedBookingAsync(db.NewContext(), reviewer);
        var otherBooking = await SeedBookingAsync(db.NewContext(), reviewer);
        var writeContext = db.NewContext();
        var matching = MakeReview(reviewer, bookingId: booking.Id);
        var nonMatching = MakeReview(reviewer, bookingId: otherBooking.Id);
        writeContext.Reviews.AddRange(matching, nonMatching);
        await writeContext.SaveChangesAsync(Ct);

        var (items, total) = await new ReviewRepository(db.NewContext()).GetPagedByServiceAsync(booking.ServiceId, 1, 20, Ct);

        Assert.Equal(1, total);
        Assert.Equal(matching.Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new ReviewRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByProductAndUserAsync_MatchesOnlyThatCombination()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        var review = MakeReview(reviewer, productId: product.Id);
        context.Reviews.Add(review);
        await context.SaveChangesAsync(Ct);
        var repository = new ReviewRepository(db.NewContext());

        Assert.NotNull(await repository.GetByProductAndUserAsync(product.Id, reviewer.Id, Ct));
        Assert.Null(await repository.GetByProductAndUserAsync(product.Id, Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByHeritagePlaceAndUserAsync_MatchesOnlyThatCombination()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var place = await SeedHeritagePlaceAsync(context);
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        context.Reviews.Add(MakeReview(reviewer, placeId: place.Id));
        await context.SaveChangesAsync(Ct);

        Assert.NotNull(await new ReviewRepository(db.NewContext()).GetByHeritagePlaceAndUserAsync(place.Id, reviewer.Id, Ct));
    }

    [Fact]
    public async Task GetByBookingAndUserAsync_MatchesOnlyThatCombination()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        await context.SaveChangesAsync(Ct);
        var booking = await SeedBookingAsync(db.NewContext(), reviewer);
        var writeContext = db.NewContext();
        writeContext.Reviews.Add(MakeReview(reviewer, bookingId: booking.Id));
        await writeContext.SaveChangesAsync(Ct);

        Assert.NotNull(await new ReviewRepository(db.NewContext()).GetByBookingAndUserAsync(booking.Id, reviewer.Id, Ct));
    }

    [Fact]
    public async Task GetAggregateAsync_ComputesTheAverageAndCountForThatProductOnly()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        var otherProduct = await SeedProductAsync(context);
        var reviewerA = TestUsers.Create();
        var reviewerB = TestUsers.Create();
        context.Users.AddRange(reviewerA, reviewerB);
        context.Reviews.AddRange(
            MakeReview(reviewerA, productId: product.Id, rating: 4),
            MakeReview(reviewerB, productId: product.Id, rating: 2),
            MakeReview(reviewerA, productId: otherProduct.Id, rating: 5));
        await context.SaveChangesAsync(Ct);

        var (average, count) = await new ReviewRepository(db.NewContext()).GetAggregateAsync(product.Id, Ct);

        Assert.Equal(2, count);
        Assert.Equal(3.0, average, 3);
    }

    [Fact]
    public async Task GetAggregateAsync_NoReviews_ReturnsZeroAndZero()
    {
        await using var db = await _database.BeginAsync();

        var (average, count) = await new ReviewRepository(db.NewContext()).GetAggregateAsync(Guid.NewGuid(), Ct);

        Assert.Equal(0, count);
        Assert.Equal(0, average);
    }

    [Fact]
    public async Task GetAggregateByHeritagePlaceAsync_ComputesTheAverageAndCount()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var place = await SeedHeritagePlaceAsync(context);
        var reviewerA = TestUsers.Create();
        var reviewerB = TestUsers.Create();
        context.Users.AddRange(reviewerA, reviewerB);
        context.Reviews.AddRange(MakeReview(reviewerA, placeId: place.Id, rating: 5), MakeReview(reviewerB, placeId: place.Id, rating: 3));
        await context.SaveChangesAsync(Ct);

        var (average, count) = await new ReviewRepository(db.NewContext()).GetAggregateByHeritagePlaceAsync(place.Id, Ct);

        Assert.Equal(2, count);
        Assert.Equal(4.0, average, 3);
    }

    [Fact]
    public async Task GetAggregateByServiceAsync_ComputesTheAverageAcrossBookingsOfThatService()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        await context.SaveChangesAsync(Ct);
        var booking = await SeedBookingAsync(db.NewContext(), reviewer);
        var writeContext = db.NewContext();
        writeContext.Reviews.Add(MakeReview(reviewer, bookingId: booking.Id, rating: 5));
        await writeContext.SaveChangesAsync(Ct);

        var (average, count) = await new ReviewRepository(db.NewContext()).GetAggregateByServiceAsync(booking.ServiceId, Ct);

        Assert.Equal(1, count);
        Assert.Equal(5.0, average, 3);
    }

    [Fact]
    public async Task GetAggregateByProducerAsync_OnlyCountsThatProducersProductsWithinThePeriod()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        var producerId = product.ProducerId;
        var reviewerA = TestUsers.Create();
        var reviewerB = TestUsers.Create();
        context.Users.AddRange(reviewerA, reviewerB);
        var inPeriod = MakeReview(reviewerA, productId: product.Id, rating: 4, createdAt: DateTime.UtcNow.AddDays(-1));
        var outOfPeriod = MakeReview(reviewerB, productId: product.Id, rating: 1, createdAt: DateTime.UtcNow.AddDays(-30));
        context.Reviews.AddRange(inPeriod, outOfPeriod);
        await context.SaveChangesAsync(Ct);

        var (average, count) = await new ReviewRepository(db.NewContext())
            .GetAggregateByProducerAsync(producerId, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, Ct);

        Assert.Equal(1, count);
        Assert.Equal(4.0, average, 3);
    }

    [Fact]
    public async Task AddImageAsync_RemoveImage_AndRemove_ModifyTheDatabaseOnSaveChanges()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var product = await SeedProductAsync(context);
        var reviewer = TestUsers.Create();
        context.Users.Add(reviewer);
        var review = MakeReview(reviewer, productId: product.Id);
        context.Reviews.Add(review);
        await context.SaveChangesAsync(Ct);

        var addRepository = new ReviewRepository(db.NewContext());
        var image = new ReviewImage { Id = Guid.NewGuid(), ReviewId = review.Id, ImageUrl = "a.jpg", DisplayOrder = 0 };
        await addRepository.AddImageAsync(image, Ct);
        await addRepository.SaveChangesAsync(Ct);
        Assert.True(await db.NewContext().ReviewImages.AnyAsync(i => i.Id == image.Id, Ct));

        var removeContext = db.NewContext();
        var removeRepository = new ReviewRepository(removeContext);
        var loadedImage = await removeContext.ReviewImages.SingleAsync(i => i.Id == image.Id, Ct);
        removeRepository.RemoveImage(loadedImage);
        await removeRepository.SaveChangesAsync(Ct);
        Assert.False(await db.NewContext().ReviewImages.AnyAsync(i => i.Id == image.Id, Ct));

        var deleteContext = db.NewContext();
        var deleteRepository = new ReviewRepository(deleteContext);
        var loadedReview = await deleteContext.Reviews.SingleAsync(r => r.Id == review.Id, Ct);
        deleteRepository.Remove(loadedReview);
        await deleteRepository.SaveChangesAsync(Ct);
        Assert.False(await db.NewContext().Reviews.AnyAsync(r => r.Id == review.Id, Ct));
    }
}
