using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Auth;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Auth.Services;

[Trait("Feature", "Auth")]
[Trait("Layer", "Service")]
public class RoleServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roles = Substitute.For<IRoleRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private RoleService CreateService() => new(_users, _roles);

    [Fact]
    public async Task AssignRoleAsync_NewRole_AddsItWithTheAdminAsAssigner()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        var producer = TestUsers.Role(RoleNames.Producer);
        var adminId = Guid.NewGuid();
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roles.GetByNameAsync(RoleNames.Producer, Arg.Any<CancellationToken>()).Returns(producer);
        var before = DateTime.UtcNow;

        await CreateService().AssignRoleAsync(user.Id, RoleNames.Producer, adminId, Ct);

        Assert.Equal(2, user.UserRoles.Count);
        var added = Assert.Single(user.UserRoles, ur => ur.RoleId == producer.Id);
        Assert.Equal(user.Id, added.UserId);
        Assert.Equal(adminId, added.AssignedByUserId);
        Assert.InRange(added.AssignedAt, before, DateTime.UtcNow);
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRoleAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().AssignRoleAsync(Guid.NewGuid(), RoleNames.Producer, Guid.NewGuid(), Ct));

        Assert.Equal("User not found.", error.Message);
        await _roles.DidNotReceive().GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRoleAsync_UnknownRole_ThrowsNotFoundAndSavesNothing()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().AssignRoleAsync(user.Id, "Ghost", Guid.NewGuid(), Ct));

        Assert.Equal("Role not found.", error.Message);
        Assert.Single(user.UserRoles);
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRoleAsync_RoleAlreadyHeld_ThrowsConflictAndSavesNothing()
    {
        var producer = TestUsers.Role(RoleNames.Producer);
        var user = TestUsers.Create().WithRoles(producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _roles.GetByNameAsync(RoleNames.Producer, Arg.Any<CancellationToken>()).Returns(producer);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().AssignRoleAsync(user.Id, RoleNames.Producer, Guid.NewGuid(), Ct));

        Assert.Equal("User already has this role.", error.Message);
        Assert.Single(user.UserRoles);
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveRoleAsync_OneOfSeveralRoles_RemovesItAndSaves()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer, RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await CreateService().RemoveRoleAsync(user.Id, RoleNames.Producer, Ct);

        Assert.Equal(RoleNames.Customer, Assert.Single(user.UserRoles).Role.Name);
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveRoleAsync_RoleNameInDifferentCase_StillMatches()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer, RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await CreateService().RemoveRoleAsync(user.Id, "PRODUCER", Ct);

        Assert.Equal(RoleNames.Customer, Assert.Single(user.UserRoles).Role.Name);
    }

    [Fact]
    public async Task RemoveRoleAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().RemoveRoleAsync(Guid.NewGuid(), RoleNames.Customer, Ct));

        Assert.Equal("User not found.", error.Message);
    }

    [Fact]
    public async Task RemoveRoleAsync_RoleNotHeld_ThrowsNotFound()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer, RoleNames.Tourist);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().RemoveRoleAsync(user.Id, RoleNames.Producer, Ct));

        Assert.Equal("User does not have this role.", error.Message);
        Assert.Equal(2, user.UserRoles.Count);
    }

    [Fact]
    public async Task RemoveRoleAsync_OnlyRemainingRole_ThrowsConflictAndKeepsIt()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().RemoveRoleAsync(user.Id, RoleNames.Customer, Ct));

        Assert.Equal("Cannot remove a user's only remaining role.", error.Message);
        Assert.Single(user.UserRoles);
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
