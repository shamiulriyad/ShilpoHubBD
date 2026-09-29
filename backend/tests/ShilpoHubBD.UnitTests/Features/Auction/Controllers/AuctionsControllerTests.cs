using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auction.Controllers;

[Trait("Feature", "Auction")]
[Trait("Layer", "Controller")]
public class AuctionsControllerTests
{
    private readonly IAuctionService _service = Substitute.For<IAuctionService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuctionsController CreateController(params string[] roles) => new AuctionsController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiAuctions()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(AuctionsController)));
        Assert.Equal("api/auctions", AccessRules.ControllerRoute(typeof(AuctionsController)));
    }

    [Theory]
    [InlineData(nameof(AuctionsController.GetAll))]
    [InlineData(nameof(AuctionsController.GetById))]
    public void PublicActions_HaveNoRoleRestriction(string action)
        => Assert.Empty(typeof(AuctionsController).GetMethod(action)!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false));

    [Theory]
    [InlineData(nameof(AuctionsController.GetMine))]
    [InlineData(nameof(AuctionsController.Create))]
    [InlineData(nameof(AuctionsController.Cancel))]
    public void ProducerActions_AreRestrictedToProducerAndSuperAdmin(string action)
        => Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}",
            typeof(AuctionsController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles);

    [Fact]
    public void PlaceBid_RequiresSignInWithoutARoleRestriction()
    {
        var attribute = typeof(AuctionsController).GetMethod(nameof(AuctionsController.PlaceBid))!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single();
        Assert.Null(attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(AuctionsController.GetAll), "GET", null)]
    [InlineData(nameof(AuctionsController.GetMine), "GET", "mine")]
    [InlineData(nameof(AuctionsController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(AuctionsController.Create), "POST", null)]
    [InlineData(nameof(AuctionsController.PlaceBid), "POST", "{id:guid}/bids")]
    [InlineData(nameof(AuctionsController.Cancel), "POST", "{id:guid}/cancel")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(AuctionsController), action));

    [Fact]
    public async Task GetAll_PassesTheQueryThroughAndReturnsOk()
    {
        var query = new AuctionQueryParameters { Status = ShilpoHubBD.Domain.Entities.Auction.AuctionStatus.Active };
        var page = new PagedResult<AuctionListItemDto>();
        _service.GetAllAsync(query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetAll(query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMine_UsesTheSignedInProducerAndPaging()
    {
        var page = new PagedResult<AuctionListItemDto> { Page = 2, PageSize = 5 };
        _service.GetMineAsync(_userId, 2, 5, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController(RoleNames.Producer).GetMine(2, 5, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_ReturnsTheAuction()
    {
        var dto = new AuctionDto { Id = Guid.NewGuid() };
        _service.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetById(dto.Id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_Unknown_PropagatesNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<AuctionDto>(_ => throw new NotFoundException("Auction not found."));

        await Assert.ThrowsAsync<NotFoundException>(() => CreateController().GetById(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Create_UsesTheSignedInProducerAndReturns201PointingAtGetById()
    {
        var request = new CreateAuctionRequest();
        var dto = new AuctionDto { Id = Guid.NewGuid() };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(AuctionsController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
    }

    [Fact]
    public async Task PlaceBid_UsesTheSignedInBidder()
    {
        var auctionId = Guid.NewGuid();
        var request = new PlaceBidRequest { Amount = 1500 };
        var dto = new AuctionDto { Id = auctionId, CurrentPrice = 1500 };
        _service.PlaceBidAsync(auctionId, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().PlaceBid(auctionId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task PlaceBid_BelowMinimum_PropagatesTheConflict()
    {
        _service.PlaceBidAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PlaceBidRequest>(), Arg.Any<CancellationToken>())
            .Returns<AuctionDto>(_ => throw new ConflictException("Your bid must be at least 1100."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().PlaceBid(Guid.NewGuid(), new PlaceBidRequest(), Ct));
    }

    [Fact]
    public async Task Cancel_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var auctionId = Guid.NewGuid();
        var dto = new AuctionDto { Id = auctionId, Status = "Cancelled" };
        _service.CancelAsync(auctionId, _userId, false, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Cancel(auctionId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Cancel_SignedInAsSuperAdmin_PassesIsAdminTrue()
    {
        var auctionId = Guid.NewGuid();

        await CreateController(RoleNames.SuperAdmin).Cancel(auctionId, Ct);

        await _service.Received(1).CancelAsync(auctionId, _userId, true, Arg.Any<CancellationToken>());
    }
}
