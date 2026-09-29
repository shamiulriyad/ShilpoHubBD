using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Controllers;

[Trait("Feature", "Reviews")]
[Trait("Layer", "Controller")]
public class ReviewsControllerTests
{
    private readonly IReviewService _service = Substitute.For<IReviewService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ReviewsController CreateController(params string[] roles) => new ReviewsController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiReviews()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(ReviewsController)));
        Assert.Equal("api/reviews", AccessRules.ControllerRoute(typeof(ReviewsController)));
    }

    [Theory]
    [InlineData(nameof(ReviewsController.GetByProduct))]
    [InlineData(nameof(ReviewsController.GetByHeritagePlace))]
    [InlineData(nameof(ReviewsController.GetByService))]
    public void ReadActions_HaveNoRoleRestriction(string action)
        => Assert.Empty(typeof(ReviewsController).GetMethod(action)!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false));

    [Theory]
    [InlineData(nameof(ReviewsController.Create))]
    [InlineData(nameof(ReviewsController.Update))]
    [InlineData(nameof(ReviewsController.Delete))]
    public void WriteActions_RequireSignInWithoutARoleRestriction(string action)
    {
        var attribute = typeof(ReviewsController).GetMethod(action)!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single();
        Assert.Null(attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(ReviewsController.GetByProduct), "GET", "product/{productId:guid}")]
    [InlineData(nameof(ReviewsController.GetByHeritagePlace), "GET", "heritage-place/{heritagePlaceId:guid}")]
    [InlineData(nameof(ReviewsController.GetByService), "GET", "service/{touristServiceId:guid}")]
    [InlineData(nameof(ReviewsController.Create), "POST", null)]
    [InlineData(nameof(ReviewsController.Update), "PUT", "{id:guid}")]
    [InlineData(nameof(ReviewsController.Delete), "DELETE", "{id:guid}")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(ReviewsController), action));

    [Fact]
    public async Task GetByProduct_PassesTheQueryThroughAndReturnsOk()
    {
        var productId = Guid.NewGuid();
        var query = new ReviewQueryParameters();
        var page = new PagedResult<ReviewDto>();
        _service.GetByProductAsync(productId, query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetByProduct(productId, query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetByHeritagePlace_ReturnsTheServicesPagedReviews()
    {
        var placeId = Guid.NewGuid();
        var page = new PagedResult<ReviewDto>();
        _service.GetByHeritagePlaceAsync(placeId, Arg.Any<ReviewQueryParameters>(), Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetByHeritagePlace(placeId, new ReviewQueryParameters(), Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetByService_ReturnsThePagedReviews()
    {
        var serviceId = Guid.NewGuid();
        var page = new PagedResult<ReviewDto>();
        _service.GetByServiceAsync(serviceId, Arg.Any<ReviewQueryParameters>(), Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetByService(serviceId, new ReviewQueryParameters(), Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Create_ProductReview_UsesTheSignedInUserAndReturns201PointingAtGetByProduct()
    {
        var request = new CreateReviewRequest { ProductId = Guid.NewGuid(), Rating = 5, Comment = "x" };
        var dto = new ReviewDto { ProductId = request.ProductId };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ReviewsController.GetByProduct), created.ActionName);
        Assert.Equal(request.ProductId, created.RouteValues?["productId"]);
    }

    [Fact]
    public async Task Create_HeritagePlaceReview_Returns201PointingAtGetByHeritagePlace()
    {
        var request = new CreateReviewRequest { HeritagePlaceId = Guid.NewGuid(), Rating = 5, Comment = "x" };
        var dto = new ReviewDto { HeritagePlaceId = request.HeritagePlaceId };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ReviewsController.GetByHeritagePlace), created.ActionName);
        Assert.Equal(request.HeritagePlaceId, created.RouteValues?["heritagePlaceId"]);
    }

    [Fact]
    public async Task Create_BookingReview_ReturnsAPlain201WithNoActionLink()
    {
        var request = new CreateReviewRequest { BookingId = Guid.NewGuid(), Rating = 5, Comment = "x" };
        var dto = new ReviewDto { BookingId = request.BookingId };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, status.StatusCode);
        Assert.Same(dto, status.Value);
    }

    [Fact]
    public async Task Create_NoSubjectSet_PropagatesTheConflict()
    {
        _service.CreateAsync(Arg.Any<Guid>(), Arg.Any<CreateReviewRequest>(), Arg.Any<CancellationToken>())
            .Returns<ReviewDto>(_ => throw new ConflictException("Exactly one of ProductId, HeritagePlaceId or BookingId must be set."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().Create(new CreateReviewRequest(), Ct));
    }

    [Fact]
    public async Task Update_UsesTheSignedInUser()
    {
        var id = Guid.NewGuid();
        var request = new UpdateReviewRequest { Rating = 4, Comment = "x" };
        var dto = new ReviewDto { Id = id, Rating = 4 };
        _service.UpdateAsync(id, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Update(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Update_AnotherUsersReview_PropagatesUnauthorized()
    {
        _service.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<UpdateReviewRequest>(), Arg.Any<CancellationToken>())
            .Returns<ReviewDto>(_ => throw new UnauthorizedAccessException("You do not have permission to edit this review."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateController().Update(Guid.NewGuid(), new UpdateReviewRequest(), Ct));
    }

    [Fact]
    public async Task Delete_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var id = Guid.NewGuid();

        var result = await CreateController().Delete(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).DeleteAsync(id, _userId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_SignedInAsSuperAdmin_PassesIsAdminTrue()
    {
        var id = Guid.NewGuid();

        await CreateController(RoleNames.SuperAdmin).Delete(id, Ct);

        await _service.Received(1).DeleteAsync(id, _userId, true, Arg.Any<CancellationToken>());
    }
}
