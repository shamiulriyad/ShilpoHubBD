using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Admin;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Admin.Services;

[Trait("Feature", "Admin")]
[Trait("Layer", "Service")]
public class AdminUserServiceTests
{
    private readonly IAdminUserRepository _users = Substitute.For<IAdminUserRepository>();
    private readonly IIdentityVerificationRepository _verifications = Substitute.For<IIdentityVerificationRepository>();
    private readonly IAuditLogService _auditLog = Substitute.For<IAuditLogService>();

    public AdminUserServiceTests()
    {
        _verifications.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<IdentityVerificationRequest>());
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AdminUserService CreateService() => new(_users, _verifications, _auditLog);

    // ---------- GetPagedAsync ----------

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 101, 2, 20)]
    [InlineData(3, 100, 3, 100)]
    public async Task GetPagedAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        var query = new AdminUserQueryParameters { Page = page, PageSize = pageSize };
        _users.GetPagedAsync(Arg.Any<AdminUserQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<User>(), 0));

        var result = await CreateService().GetPagedAsync(query, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
    }

    [Fact]
    public async Task GetPagedAsync_MapsUsersWithRolesAndVerificationStatus()
    {
        var user = TestUsers.Create(fullName: "Rahima Begum").WithRoles(RoleNames.Producer);
        _users.GetPagedAsync(Arg.Any<AdminUserQueryParameters>(), Arg.Any<CancellationToken>())
            .Returns((new List<User> { user }, 1));
        _verifications.GetLatestStatusesByUserIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, IdentityVerificationStatus> { [user.Id] = IdentityVerificationStatus.Approved });

        var result = await CreateService().GetPagedAsync(new AdminUserQueryParameters(), Ct);

        var dto = Assert.Single(result.Items);
        Assert.Equal("Rahima Begum", dto.FullName);
        Assert.Equal(new[] { RoleNames.Producer }, dto.Roles);
        Assert.Equal("Approved", dto.IdentityVerificationStatus);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_UserWithNoVerificationHistory_ReportsStatusNone()
    {
        var user = TestUsers.Create();
        _users.GetPagedAsync(Arg.Any<AdminUserQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<User> { user }, 1));
        _verifications.GetLatestStatusesByUserIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, IdentityVerificationStatus>());

        var result = await CreateService().GetPagedAsync(new AdminUserQueryParameters(), Ct);

        Assert.Equal("None", Assert.Single(result.Items).IdentityVerificationStatus);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDetailWithVerificationHistory()
    {
        var user = TestUsers.Create(fullName: "Karim Sheikh").WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var verification = new IdentityVerificationRequest
        {
            Id = Guid.NewGuid(), UserId = user.Id, User = user, DocumentNumber = "DOC-1", FrontImageUrl = "front.jpg",
        };
        _verifications.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(new List<IdentityVerificationRequest> { verification });

        var dto = await CreateService().GetByIdAsync(user.Id, Ct);

        Assert.Equal("Karim Sheikh", dto.FullName);
        Assert.Equal(new[] { RoleNames.Customer }, dto.Roles);
        Assert.Equal(verification.Id, Assert.Single(dto.IdentityVerifications).Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(Guid.NewGuid(), Ct));

        Assert.Equal("User not found.", error.Message);
    }

    // ---------- SetActiveAsync ----------

    [Fact]
    public async Task SetActiveAsync_Activate_UpdatesTheUserAndLogsAudit()
    {
        var user = TestUsers.Create(fullName: "Karim Sheikh", isActive: false);
        var admin = TestUsers.Create(fullName: "Admin Person");
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _users.GetByIdWithRolesAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        var before = DateTime.UtcNow;

        var dto = await CreateService().SetActiveAsync(user.Id, true, admin.Id, "203.0.113.9", Ct);

        Assert.True(user.IsActive);
        Assert.InRange(user.UpdatedAt, before, DateTime.UtcNow);
        Assert.True(dto.IsActive);
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _auditLog.Received(1).LogAsync(
            admin.Id, "Admin Person", "AdminUser.Activated", "User", user.Id,
            "Activated account for Karim Sheikh.", "203.0.113.9", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetActiveAsync_Deactivate_UpdatesTheUserAndLogsDeactivation()
    {
        var user = TestUsers.Create(fullName: "Karim Sheikh");
        var admin = TestUsers.Create(fullName: "Admin Person");
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _users.GetByIdWithRolesAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);

        var dto = await CreateService().SetActiveAsync(user.Id, false, admin.Id, null, Ct);

        Assert.False(user.IsActive);
        Assert.False(dto.IsActive);
        await _auditLog.Received(1).LogAsync(
            admin.Id, "Admin Person", "AdminUser.Deactivated", "User", user.Id,
            "Deactivated account for Karim Sheikh.", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetActiveAsync_ActorLookupFails_FallsBackToTheActorIdAsTheAuditName()
    {
        var user = TestUsers.Create(fullName: "Karim Sheikh");
        var adminId = Guid.NewGuid();
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _users.GetByIdWithRolesAsync(adminId, Arg.Any<CancellationToken>()).Returns((User?)null);

        await CreateService().SetActiveAsync(user.Id, true, adminId, null, Ct);

        await _auditLog.Received(1).LogAsync(
            adminId, adminId.ToString(), Arg.Any<string>(), "User", user.Id, Arg.Any<string>(),
            Arg.Is<string?>(ip => ip == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetActiveAsync_UnknownUser_ThrowsNotFoundAndSavesNothing()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().SetActiveAsync(Guid.NewGuid(), true, Guid.NewGuid(), null, Ct));

        Assert.Equal("User not found.", error.Message);
        await _users.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _auditLog.DidNotReceive().LogAsync(
            Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
