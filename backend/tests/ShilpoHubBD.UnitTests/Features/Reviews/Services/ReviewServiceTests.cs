using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Domain.Entities.TouristBooking;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Services;

[Trait("Feature", "Reviews")]
[Trait("Layer", "Service")]
public class ReviewServiceTests
{
    private readonly IReviewRepository _reviews = Substitute.For<IReviewRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IHeritagePlaceRepository _places = Substitute.For<IHeritagePlaceRepository>();
    private readonly IHeritageCheckInRepository _checkIns = Substitute.For<IHeritageCheckInRepository>();
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly ITouristServiceRepository _services = Substitute.For<ITouristServiceRepository>();
    private readonly IOrderComplaintRepository _complaints = Substitute.For<IOrderComplaintRepository>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ReviewService CreateService() => new(_reviews, _products, _orders, _places, _checkIns, _bookings, _services, _complaints);

    /// <summary>Makes AddAsync capture the review and GetByIdAsync return it (with a reviewer set) on re-fetch.</summary>
    private Review? StubRoundTrip()
    {
        Review? saved = null;
        _reviews.AddAsync(Arg.Do<Review>(r => { r.User = TestUsers.Create(fullName: "Rahima Begum"); saved = r; }), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _reviews.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => saved);
        return saved;
    }

    // ---------- GetByProductAsync / GetByHeritagePlaceAsync / GetByServiceAsync ----------

    [Fact]
    public async Task GetByProductAsync_MapsItemsAndTotalCount()
    {
        var review = new Review { Id = Guid.NewGuid(), Rating = 5, Comment = "x", User = TestUsers.Create(fullName: "Rahima Begum") };
        _reviews.GetPagedByProductAsync(Arg.Any<Guid>(), 1, 10, Arg.Any<CancellationToken>()).Returns((new List<Review> { review }, 9));

        var result = await CreateService().GetByProductAsync(Guid.NewGuid(), new ReviewQueryParameters(), Ct);

        Assert.Equal(9, result.TotalCount);
        Assert.Equal("Rahima Begum", Assert.Single(result.Items).ReviewerName);
    }

    [Fact]
    public async Task GetByHeritagePlaceAsync_PassesThePagingThrough()
    {
        _reviews.GetPagedByHeritagePlaceAsync(Arg.Any<Guid>(), 2, 5, Arg.Any<CancellationToken>()).Returns((new List<Review>(), 0));

        await CreateService().GetByHeritagePlaceAsync(Guid.NewGuid(), new ReviewQueryParameters { Page = 2, PageSize = 5 }, Ct);

        await _reviews.Received(1).GetPagedByHeritagePlaceAsync(Arg.Any<Guid>(), 2, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByServiceAsync_PassesThePagingThrough()
    {
        _reviews.GetPagedByServiceAsync(Arg.Any<Guid>(), 3, 8, Arg.Any<CancellationToken>()).Returns((new List<Review>(), 0));

        await CreateService().GetByServiceAsync(Guid.NewGuid(), new ReviewQueryParameters { Page = 3, PageSize = 8 }, Ct);

        await _reviews.Received(1).GetPagedByServiceAsync(Arg.Any<Guid>(), 3, 8, Arg.Any<CancellationToken>());
    }

    // ---------- CreateAsync: routing ----------

    [Fact]
    public async Task CreateAsync_NoSubjectSet_ThrowsConflict()
    {
        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().CreateAsync(_userId, new CreateReviewRequest { Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("Exactly one of ProductId, HeritagePlaceId or BookingId must be set.", error.Message);
    }

    // ---------- CreateAsync: product ----------

    [Fact]
    public async Task CreateAsync_ProductReview_EligibleBuyer_CreatesItAttachesImagesAndRecalculatesTheProductRating()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, AverageRating = 0, ReviewCount = 0 };
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _orders.HasPurchasedProductAsync(_userId, productId, Arg.Any<CancellationToken>()).Returns(true);
        _reviews.GetAggregateAsync(productId, Arg.Any<CancellationToken>()).Returns((4.5, 2));
        StubRoundTrip();
        ReviewImage? image = null;
        await _reviews.AddImageAsync(Arg.Do<ReviewImage>(i => image = i), Arg.Any<CancellationToken>());
        var request = new CreateReviewRequest
        {
            ProductId = productId, Rating = 5, ProducerRating = 4, Comment = "  Lovely weave.  ", ImageUrls = new() { " a.jpg " },
        };
        var before = DateTime.UtcNow;

        var dto = await CreateService().CreateAsync(_userId, request, Ct);

        Assert.Equal(productId, dto.ProductId);
        Assert.Equal(5, dto.Rating);
        Assert.Equal(4, dto.ProducerRating);
        Assert.Equal("Rahima Begum", dto.ReviewerName);
        Assert.NotNull(image);
        Assert.Equal("a.jpg", image.ImageUrl);
        Assert.Equal(0, image.DisplayOrder);
        Assert.Equal(4.5m, product.AverageRating);
        Assert.Equal(2, product.ReviewCount);
        await _products.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _reviews.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.InRange(before, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_ProductReview_UnknownProduct_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { ProductId = Guid.NewGuid(), Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("Product not found.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_ProductReview_NotPurchased_ThrowsConflictAndSavesNothing()
    {
        var productId = Guid.NewGuid();
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new Product { Id = productId });
        _orders.HasPurchasedProductAsync(_userId, productId, Arg.Any<CancellationToken>()).Returns(false);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { ProductId = productId, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You can only review products you have purchased and received.", error.Message);
        await _reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ProductReview_UnsettledComplaintExists_ThrowsConflict()
    {
        var productId = Guid.NewGuid();
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new Product { Id = productId });
        _orders.HasPurchasedProductAsync(_userId, productId, Arg.Any<CancellationToken>()).Returns(true);
        _complaints.HasUnsettledAsync(_userId, productId, Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { ProductId = productId, Rating = 5, Comment = "x" }, Ct));

        Assert.Contains("complaint about this product", error.Message);
    }

    [Fact]
    public async Task CreateAsync_ProductReview_AlreadyReviewed_ThrowsConflict()
    {
        var productId = Guid.NewGuid();
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new Product { Id = productId });
        _orders.HasPurchasedProductAsync(_userId, productId, Arg.Any<CancellationToken>()).Returns(true);
        _reviews.GetByProductAndUserAsync(productId, _userId, Arg.Any<CancellationToken>()).Returns(new Review());

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { ProductId = productId, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You have already reviewed this product. Edit your existing review instead.", error.Message);
    }

    // ---------- CreateAsync: heritage place ----------

    [Fact]
    public async Task CreateAsync_HeritagePlaceReview_CheckedIn_CreatesItAndRecalculatesTheRating()
    {
        var placeId = Guid.NewGuid();
        var place = new HeritagePlace { Id = placeId, AverageRating = 0, ReviewCount = 0 };
        _places.GetByIdAsync(placeId, Arg.Any<CancellationToken>()).Returns(place);
        _checkIns.HasCheckedInAsync(_userId, placeId, Arg.Any<CancellationToken>()).Returns(true);
        _reviews.GetAggregateByHeritagePlaceAsync(placeId, Arg.Any<CancellationToken>()).Returns((5.0, 1));
        StubRoundTrip();

        var dto = await CreateService().CreateAsync(_userId, new CreateReviewRequest { HeritagePlaceId = placeId, Rating = 5, Comment = "x" }, Ct);

        Assert.Equal(placeId, dto.HeritagePlaceId);
        Assert.Equal(5.0m, place.AverageRating);
        await _places.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_HeritagePlaceReview_UnknownPlace_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { HeritagePlaceId = Guid.NewGuid(), Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("Heritage place not found.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_HeritagePlaceReview_NotCheckedIn_ThrowsConflict()
    {
        var placeId = Guid.NewGuid();
        _places.GetByIdAsync(placeId, Arg.Any<CancellationToken>()).Returns(new HeritagePlace { Id = placeId });
        _checkIns.HasCheckedInAsync(_userId, placeId, Arg.Any<CancellationToken>()).Returns(false);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { HeritagePlaceId = placeId, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You can only review heritage places you have checked in to.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_HeritagePlaceReview_AlreadyReviewed_ThrowsConflict()
    {
        var placeId = Guid.NewGuid();
        _places.GetByIdAsync(placeId, Arg.Any<CancellationToken>()).Returns(new HeritagePlace { Id = placeId });
        _checkIns.HasCheckedInAsync(_userId, placeId, Arg.Any<CancellationToken>()).Returns(true);
        _reviews.GetByHeritagePlaceAndUserAsync(placeId, _userId, Arg.Any<CancellationToken>()).Returns(new Review());

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { HeritagePlaceId = placeId, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You have already reviewed this heritage place. Edit your existing review instead.", error.Message);
    }

    // ---------- CreateAsync: booking ----------

    private Booking MakeBooking(Guid touristId, BookingStatus status = BookingStatus.Completed)
    {
        var booking = new Booking { Id = Guid.NewGuid(), TouristId = touristId, Status = status, ServiceId = Guid.NewGuid() };
        _bookings.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(booking);
        return booking;
    }

    [Fact]
    public async Task CreateAsync_BookingReview_CompletedOwnBooking_CreatesItAndRecalculatesTheServiceRating()
    {
        var booking = MakeBooking(_userId);
        var service = new TouristService { Id = booking.ServiceId, AverageRating = 0, ReviewCount = 0 };
        _services.GetByIdAsync(booking.ServiceId, Arg.Any<CancellationToken>()).Returns(service);
        _reviews.GetAggregateByServiceAsync(booking.ServiceId, Arg.Any<CancellationToken>()).Returns((4.0, 3));
        StubRoundTrip();

        var dto = await CreateService().CreateAsync(_userId, new CreateReviewRequest { BookingId = booking.Id, Rating = 4, Comment = "x" }, Ct);

        Assert.Equal(booking.Id, dto.BookingId);
        Assert.Equal(4.0m, service.AverageRating);
        await _services.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_BookingReview_UnknownBooking_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { BookingId = Guid.NewGuid(), Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("Booking not found.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_BookingReview_AnotherTouristsBooking_ThrowsUnauthorized()
    {
        var booking = MakeBooking(Guid.NewGuid());

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { BookingId = booking.Id, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You do not have permission to review this booking.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_BookingReview_NotYetCompleted_ThrowsConflict()
    {
        var booking = MakeBooking(_userId, BookingStatus.Confirmed);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { BookingId = booking.Id, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You can only review completed bookings.", error.Message);
    }

    [Fact]
    public async Task CreateAsync_BookingReview_AlreadyReviewed_ThrowsConflict()
    {
        var booking = MakeBooking(_userId);
        _reviews.GetByBookingAndUserAsync(booking.Id, _userId, Arg.Any<CancellationToken>()).Returns(new Review());

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().CreateAsync(_userId, new CreateReviewRequest { BookingId = booking.Id, Rating = 5, Comment = "x" }, Ct));

        Assert.Equal("You have already reviewed this booking. Edit your existing review instead.", error.Message);
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_OwnProductReview_UpdatesFieldsReplacesImagesAndRecalculatesTheRating()
    {
        var productId = Guid.NewGuid();
        var oldImage = new ReviewImage { Id = Guid.NewGuid(), ImageUrl = "old.jpg" };
        var review = new Review
        {
            Id = Guid.NewGuid(), UserId = _userId, ProductId = productId, Rating = 3, Comment = "old",
            User = TestUsers.Create(fullName: "Rahima Begum"),
        };
        review.Images.Add(oldImage);
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new Product { Id = productId });
        _reviews.GetAggregateAsync(productId, Arg.Any<CancellationToken>()).Returns((5.0, 1));
        var request = new UpdateReviewRequest { Rating = 5, Comment = "  Even better now.  ", ImageUrls = new() { "new.jpg" } };
        var before = DateTime.UtcNow;

        var dto = await CreateService().UpdateAsync(review.Id, _userId, request, Ct);

        Assert.Equal(5, review.Rating);
        Assert.Equal("Even better now.", review.Comment);
        Assert.InRange(review.UpdatedAt, before, DateTime.UtcNow);
        _reviews.Received(1).RemoveImage(oldImage);
        await _reviews.Received(1).AddImageAsync(Arg.Is<ReviewImage>(i => i.ImageUrl == "new.jpg"), Arg.Any<CancellationToken>());
        Assert.Equal(5, dto.Rating);
    }

    [Fact]
    public async Task UpdateAsync_UnknownReview_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().UpdateAsync(Guid.NewGuid(), _userId, new UpdateReviewRequest(), Ct));

        Assert.Equal("Review not found.", error.Message);
    }

    [Fact]
    public async Task UpdateAsync_AnotherUsersReview_ThrowsUnauthorized()
    {
        var review = new Review { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().UpdateAsync(review.Id, _userId, new UpdateReviewRequest(), Ct));

        Assert.Equal("You do not have permission to edit this review.", error.Message);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_OwnProductReview_RemovesItAndRecalculatesTheProductRating()
    {
        var productId = Guid.NewGuid();
        var review = new Review { Id = Guid.NewGuid(), UserId = _userId, ProductId = productId };
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);
        _products.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new Product { Id = productId });
        _reviews.GetAggregateAsync(productId, Arg.Any<CancellationToken>()).Returns((0.0, 0));

        await CreateService().DeleteAsync(review.Id, _userId, false, Ct);

        _reviews.Received(1).Remove(review);
        await _reviews.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _products.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_OwnBookingReview_RecalculatesTheServiceRatingUsingTheBookingsService()
    {
        var booking = MakeBooking(_userId);
        var review = new Review { Id = Guid.NewGuid(), UserId = _userId, BookingId = booking.Id };
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);
        _services.GetByIdAsync(booking.ServiceId, Arg.Any<CancellationToken>()).Returns(new TouristService { Id = booking.ServiceId });
        _reviews.GetAggregateByServiceAsync(booking.ServiceId, Arg.Any<CancellationToken>()).Returns((0.0, 0));

        await CreateService().DeleteAsync(review.Id, _userId, false, Ct);

        await _services.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Admin_CanDeleteAnotherUsersReview()
    {
        var review = new Review { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);

        await CreateService().DeleteAsync(review.Id, Guid.NewGuid(), true, Ct);

        _reviews.Received(1).Remove(review);
    }

    [Fact]
    public async Task DeleteAsync_AnotherUsersReviewWithoutAdmin_ThrowsUnauthorizedAndDeletesNothing()
    {
        var review = new Review { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _reviews.GetByIdAsync(review.Id, Arg.Any<CancellationToken>()).Returns(review);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().DeleteAsync(review.Id, _userId, false, Ct));

        Assert.Equal("You do not have permission to delete this review.", error.Message);
        _reviews.DidNotReceive().Remove(Arg.Any<Review>());
    }

    [Fact]
    public async Task DeleteAsync_UnknownReview_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().DeleteAsync(Guid.NewGuid(), _userId, false, Ct));

        Assert.Equal("Review not found.", error.Message);
    }
}
