using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Security;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.UnitTests.Features.Security.Services;

[Trait("Feature", "Security")]
[Trait("Layer", "Service")]
public class AuditLogServiceTests
{
    private readonly IAuditLogRepository _logs = Substitute.For<IAuditLogRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuditLogService CreateService() => new(_logs);

    [Fact]
    public async Task LogAsync_StoresEveryDetailWithATimestamp()
    {
        AuditLog? saved = null;
        await _logs.AddAsync(Arg.Do<AuditLog>(l => saved = l), Arg.Any<CancellationToken>());
        var actorId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        await CreateService().LogAsync(actorId, "Admin Person", "Product.Approved", "Product", entityId,
            "Approved Jamdani Saree.", "203.0.113.7", Ct);

        Assert.NotNull(saved);
        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal(actorId, saved.ActorUserId);
        Assert.Equal("Admin Person", saved.ActorName);
        Assert.Equal("Product.Approved", saved.Action);
        Assert.Equal("Product", saved.EntityType);
        Assert.Equal(entityId, saved.EntityId);
        Assert.Equal("Approved Jamdani Saree.", saved.Description);
        Assert.Equal("203.0.113.7", saved.IpAddress);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _logs.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogAsync_SystemActionWithoutActorEntityOrIp_IsStillRecorded()
    {
        AuditLog? saved = null;
        await _logs.AddAsync(Arg.Do<AuditLog>(l => saved = l), Arg.Any<CancellationToken>());

        await CreateService().LogAsync(null, "System", "Backup.Scheduled", "Backup", null, "Nightly backup.", null, Ct);

        Assert.NotNull(saved);
        Assert.Null(saved.ActorUserId);
        Assert.Null(saved.EntityId);
        Assert.Null(saved.IpAddress);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(4, 0, 4, 20)]
    [InlineData(4, 101, 4, 20)]
    [InlineData(2, 100, 2, 100)]
    public async Task GetPagedAsync_KeepsPageAndSizeWithinBoundsBeforeQuerying(int page, int pageSize, int expectedPage, int expectedSize)
    {
        var query = new AuditLogQueryParameters { Page = page, PageSize = pageSize, Action = "Product.Approved" };
        _logs.GetPagedAsync(query, Arg.Any<CancellationToken>()).Returns((new List<AuditLog>(), 0));

        var result = await CreateService().GetPagedAsync(query, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
        await _logs.Received(1).GetPagedAsync(
            Arg.Is<AuditLogQueryParameters>(q => q.Page == expectedPage && q.PageSize == expectedSize && q.Action == "Product.Approved"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_MapsEveryFieldAndTheTotalCount()
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = Guid.NewGuid(),
            ActorName = "Admin Person",
            Action = "AdminUser.Deactivated",
            EntityType = "User",
            EntityId = Guid.NewGuid(),
            Description = "Deactivated a user.",
            IpAddress = "198.51.100.1",
            CreatedAt = DateTime.UtcNow.AddMinutes(-2),
        };
        var query = new AuditLogQueryParameters();
        _logs.GetPagedAsync(query, Arg.Any<CancellationToken>()).Returns((new List<AuditLog> { log }, 7));

        var result = await CreateService().GetPagedAsync(query, Ct);

        Assert.Equal(7, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(log.Id, dto.Id);
        Assert.Equal(log.ActorUserId, dto.ActorUserId);
        Assert.Equal(log.ActorName, dto.ActorName);
        Assert.Equal(log.Action, dto.Action);
        Assert.Equal(log.EntityType, dto.EntityType);
        Assert.Equal(log.EntityId, dto.EntityId);
        Assert.Equal(log.Description, dto.Description);
        Assert.Equal(log.IpAddress, dto.IpAddress);
        Assert.Equal(log.CreatedAt, dto.CreatedAt);
    }
}
