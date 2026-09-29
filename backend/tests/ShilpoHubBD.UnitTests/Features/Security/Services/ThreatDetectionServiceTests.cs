using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Security;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Services;

[Trait("Feature", "Security")]
[Trait("Layer", "Service")]
public class ThreatDetectionServiceTests
{
    private readonly IThreatDetectionRepository _threats = Substitute.For<IThreatDetectionRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly User _admin = TestUsers.Create(fullName: "Admin Person");

    public ThreatDetectionServiceTests()
    {
        _users.GetByIdAsync(_admin.Id, Arg.Any<CancellationToken>()).Returns(_admin);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ThreatDetectionService CreateService() => new(_threats, _users);

    // ---------- GetFailedLoginsAsync ----------

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 101, 2, 20)]
    [InlineData(5, 100, 5, 100)]
    public async Task GetFailedLoginsAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _threats.GetFailedLoginsPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<LoginAttempt>(), 0));

        var result = await CreateService().GetFailedLoginsAsync(page, pageSize, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
        await _threats.Received(1).GetFailedLoginsPagedAsync(expectedPage, expectedSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFailedLoginsAsync_MapsAttemptsAndTotalCount()
    {
        var attempt = new LoginAttempt
        {
            Id = Guid.NewGuid(), Email = "victim@example.com", IpAddress = "203.0.113.9", Succeeded = false, CreatedAt = DateTime.UtcNow,
        };
        _threats.GetFailedLoginsPagedAsync(1, 20, Arg.Any<CancellationToken>()).Returns((new List<LoginAttempt> { attempt }, 15));

        var result = await CreateService().GetFailedLoginsAsync(1, 20, Ct);

        Assert.Equal(15, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(attempt.Id, dto.Id);
        Assert.Equal("victim@example.com", dto.Email);
        Assert.Equal("203.0.113.9", dto.IpAddress);
        Assert.False(dto.Succeeded);
        Assert.Equal(attempt.CreatedAt, dto.CreatedAt);
    }

    // ---------- GetSuspiciousIpsAsync ----------

    [Fact]
    public async Task GetSuspiciousIpsAsync_LooksAtTheLastHourWithAtLeastFiveFailures()
    {
        DateTime? since = null;
        _threats.GetSuspiciousIpsAsync(Arg.Do<DateTime>(s => since = s), 5, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)>());
        var before = DateTime.UtcNow;

        await CreateService().GetSuspiciousIpsAsync(Ct);

        Assert.NotNull(since);
        Assert.InRange(since.Value, before.AddHours(-1), DateTime.UtcNow.AddHours(-1));
        await _threats.Received(1).GetSuspiciousIpsAsync(Arg.Any<DateTime>(), 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSuspiciousIpsAsync_MapsEachIpWithItsFailureCountInOrder()
    {
        _threats.GetSuspiciousIpsAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)> { ("203.0.113.1", 12), ("203.0.113.2", 6) });

        var result = await CreateService().GetSuspiciousIpsAsync(Ct);

        Assert.Collection(result,
            first => { Assert.Equal("203.0.113.1", first.IpAddress); Assert.Equal(12, first.FailedAttempts); },
            second => { Assert.Equal("203.0.113.2", second.IpAddress); Assert.Equal(6, second.FailedAttempts); });
    }

    // ---------- GetBlockedIpsAsync ----------

    [Fact]
    public async Task GetBlockedIpsAsync_MapsBlocksWithWhoBlockedThem()
    {
        var blocked = new BlockedIpAddress
        {
            Id = Guid.NewGuid(), IpAddress = "203.0.113.5", Reason = "Brute force", BlockedBy = _admin,
            ExpiresAt = DateTime.UtcNow.AddDays(1), CreatedAt = DateTime.UtcNow.AddHours(-1),
        };
        _threats.GetBlockedIpsAsync(Arg.Any<CancellationToken>()).Returns(new List<BlockedIpAddress> { blocked });

        var dto = Assert.Single(await CreateService().GetBlockedIpsAsync(Ct));

        Assert.Equal(blocked.Id, dto.Id);
        Assert.Equal("203.0.113.5", dto.IpAddress);
        Assert.Equal("Brute force", dto.Reason);
        Assert.Equal("Admin Person", dto.BlockedByName);
        Assert.Equal(blocked.ExpiresAt, dto.ExpiresAt);
        Assert.Equal(blocked.CreatedAt, dto.CreatedAt);
    }

    // ---------- BlockIpAsync ----------

    [Fact]
    public async Task BlockIpAsync_NewIp_SavesATrimmedBlockAttributedToTheAdmin()
    {
        BlockedIpAddress? saved = null;
        await _threats.AddBlockedIpAsync(Arg.Do<BlockedIpAddress>(b => saved = b), Arg.Any<CancellationToken>());
        var expiry = DateTime.UtcNow.AddDays(7);
        var before = DateTime.UtcNow;

        var dto = await CreateService().BlockIpAsync(
            _admin.Id, new BlockIpRequest { IpAddress = "  203.0.113.7 ", Reason = "  Brute force  ", ExpiresAt = expiry }, Ct);

        Assert.NotNull(saved);
        Assert.Equal("203.0.113.7", saved.IpAddress);
        Assert.Equal("Brute force", saved.Reason);
        Assert.Equal(_admin.Id, saved.BlockedByUserId);
        Assert.Equal(expiry, saved.ExpiresAt);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _threats.Received(1).GetBlockedIpByAddressAsync("203.0.113.7", Arg.Any<CancellationToken>());
        await _threats.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("203.0.113.7", dto.IpAddress);
        Assert.Equal("Admin Person", dto.BlockedByName);
    }

    [Fact]
    public async Task BlockIpAsync_IpAlreadyBlocked_ThrowsConflictAndSavesNothing()
    {
        _threats.GetBlockedIpByAddressAsync("203.0.113.7", Arg.Any<CancellationToken>())
            .Returns(new BlockedIpAddress { IpAddress = "203.0.113.7" });

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().BlockIpAsync(
            _admin.Id, new BlockIpRequest { IpAddress = " 203.0.113.7 ", Reason = "again" }, Ct));

        Assert.Equal("'203.0.113.7' is already blocked.", error.Message);
        await _threats.DidNotReceive().AddBlockedIpAsync(Arg.Any<BlockedIpAddress>(), Arg.Any<CancellationToken>());
        await _threats.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BlockIpAsync_UnknownAdmin_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().BlockIpAsync(
            Guid.NewGuid(), new BlockIpRequest { IpAddress = "203.0.113.7", Reason = "x" }, Ct));

        Assert.Equal("User not found.", error.Message);
        await _threats.DidNotReceive().AddBlockedIpAsync(Arg.Any<BlockedIpAddress>(), Arg.Any<CancellationToken>());
    }

    // ---------- UnblockIpAsync ----------

    [Fact]
    public async Task UnblockIpAsync_BlockedIp_RemovesTheBlockUsingTheTrimmedAddress()
    {
        var blocked = new BlockedIpAddress { Id = Guid.NewGuid(), IpAddress = "203.0.113.7" };
        _threats.GetBlockedIpByAddressAsync("203.0.113.7", Arg.Any<CancellationToken>()).Returns(blocked);

        await CreateService().UnblockIpAsync(" 203.0.113.7 ", Ct);

        _threats.Received(1).RemoveBlockedIp(blocked);
        await _threats.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnblockIpAsync_IpNotBlocked_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().UnblockIpAsync("203.0.113.7", Ct));

        Assert.Equal("That IP address is not currently blocked.", error.Message);
        _threats.DidNotReceive().RemoveBlockedIp(Arg.Any<BlockedIpAddress>());
    }
}
