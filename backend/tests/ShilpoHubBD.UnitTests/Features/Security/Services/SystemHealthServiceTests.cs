using System.Runtime.InteropServices;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Security;

namespace ShilpoHubBD.UnitTests.Features.Security.Services;

[Trait("Feature", "Security")]
[Trait("Layer", "Service")]
public class SystemHealthServiceTests
{
    private readonly ISystemHealthRepository _repository = Substitute.For<ISystemHealthRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetHealthAsync_DatabaseReachable_ReportsConnectedWithRecordCounts()
    {
        _repository.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetCountsAsync(Arg.Any<CancellationToken>()).Returns((12, 34, 56));

        var health = await new SystemHealthService(_repository).GetHealthAsync(Ct);

        Assert.True(health.DatabaseConnected);
        Assert.Equal(12, health.UserCount);
        Assert.Equal(34, health.OrderCount);
        Assert.Equal(56, health.ProductCount);
    }

    [Fact]
    public async Task GetHealthAsync_DatabaseUnreachable_ReportsZeroCountsWithoutQueryingThem()
    {
        _repository.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(false);

        var health = await new SystemHealthService(_repository).GetHealthAsync(Ct);

        Assert.False(health.DatabaseConnected);
        Assert.Equal(0, health.UserCount);
        Assert.Equal(0, health.OrderCount);
        Assert.Equal(0, health.ProductCount);
        await _repository.DidNotReceive().GetCountsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAsync_ReportsTheRunningProcessAndMachine()
    {
        _repository.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(false);
        var before = DateTime.UtcNow;

        var health = await new SystemHealthService(_repository).GetHealthAsync(Ct);

        Assert.True(health.Uptime > TimeSpan.Zero);
        Assert.True(health.WorkingSetBytes > 0);
        Assert.Equal(Environment.MachineName, health.MachineName);
        Assert.Equal(RuntimeInformation.FrameworkDescription, health.RuntimeVersion);
        Assert.InRange(health.GeneratedAt, before, DateTime.UtcNow);
    }
}
