using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Controllers;

[Trait("Feature", "Complaints")]
[Trait("Layer", "Controller")]
public class OrderComplaintsControllerTests
{
    private readonly IOrderComplaintService _service = Substitute.For<IOrderComplaintService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private OrderComplaintsController CreateController(params string[] roles) => new OrderComplaintsController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_RequiresSignInAtTheClassLevelWithNoRoleRestriction()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(OrderComplaintsController)));
        Assert.Null(AccessRules.ClassRoles(typeof(OrderComplaintsController)));
        Assert.Equal("api/order-complaints", AccessRules.ControllerRoute(typeof(OrderComplaintsController)));
    }

    [Theory]
    [InlineData(nameof(OrderComplaintsController.Received))]
    [InlineData(nameof(OrderComplaintsController.Respond))]
    public void ProducerFacingActions_RestrictToProducerOrSuperAdmin(string action)
    {
        var attribute = typeof(OrderComplaintsController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}", attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(OrderComplaintsController.Create), "POST", null)]
    [InlineData(nameof(OrderComplaintsController.Mine), "GET", "mine")]
    [InlineData(nameof(OrderComplaintsController.Received), "GET", "received")]
    [InlineData(nameof(OrderComplaintsController.Respond), "POST", "{id:guid}/respond")]
    [InlineData(nameof(OrderComplaintsController.Satisfied), "POST", "{id:guid}/satisfied")]
    [InlineData(nameof(OrderComplaintsController.Reopen), "POST", "{id:guid}/reopen")]
    [InlineData(nameof(OrderComplaintsController.Withdraw), "POST", "{id:guid}/withdraw")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(OrderComplaintsController), action));

    [Fact]
    public async Task Create_UsesTheSignedInUserAsTheCustomer()
    {
        var request = new CreateOrderComplaintRequest();
        var dto = new OrderComplaintDto { Id = Guid.NewGuid() };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Create(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Mine_ReturnsTheSignedInCustomersComplaints()
    {
        var list = new List<OrderComplaintDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMineAsCustomerAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController().Mine(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Received_ReturnsTheSignedInProducersComplaints()
    {
        var list = new List<OrderComplaintDto> { new() { Id = Guid.NewGuid() } };
        _service.GetMineAsProducerAsync(_userId, Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController(RoleNames.Producer).Received(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Respond_PassesTheSignedInProducerAndRequestThrough()
    {
        var id = Guid.NewGuid();
        var request = new RespondToOrderComplaintRequest { Message = "Fixed" };
        var dto = new OrderComplaintDto { Id = id };
        _service.RespondAsync(id, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Respond(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Satisfied_PassesTheSignedInCustomerAndRequestThrough()
    {
        var id = Guid.NewGuid();
        var request = new CustomerComplaintNoteRequest { Note = "Thanks" };
        var dto = new OrderComplaintDto { Id = id };
        _service.ConfirmSatisfiedAsync(id, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Satisfied(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Reopen_PassesTheSignedInCustomerAndRequestThrough()
    {
        var id = Guid.NewGuid();
        var request = new CustomerComplaintNoteRequest { Note = "Still broken" };
        var dto = new OrderComplaintDto { Id = id };
        _service.ReopenAsync(id, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Reopen(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Withdraw_PassesTheSignedInCustomer()
    {
        var id = Guid.NewGuid();
        var dto = new OrderComplaintDto { Id = id };
        _service.WithdrawAsync(id, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Withdraw(id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
