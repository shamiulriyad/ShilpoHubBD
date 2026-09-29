using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Controllers;

[Trait("Feature", "Security")]
[Trait("Layer", "Controller")]
public class BackupsControllerTests
{
    private readonly IBackupService _service = Substitute.For<IBackupService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private BackupsController CreateController() => new BackupsController(_service).WithUser(_adminId, RoleNames.SuperAdmin);

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheSecurityBackupsRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(BackupsController)));
        Assert.Equal("api/admin/security/backups", AccessRules.ControllerRoute(typeof(BackupsController)));
        Assert.All(AccessRules.PublicActions(typeof(BackupsController)),
            action => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(BackupsController), action)));
    }

    [Theory]
    [InlineData(nameof(BackupsController.Trigger), "POST", null)]
    [InlineData(nameof(BackupsController.GetPaged), "GET", null)]
    [InlineData(nameof(BackupsController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(BackupsController.Delete), "DELETE", "{id:guid}")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(BackupsController), action));

    [Fact]
    public async Task Trigger_StartsABackupForTheSignedInAdminAndReturns201PointingAtIt()
    {
        var dto = new BackupRecordDto { Id = Guid.NewGuid() };
        _service.TriggerBackupAsync(_adminId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Trigger(Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(BackupsController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task GetPaged_PassesPagingThroughAndReturnsOk()
    {
        var page = new PagedResult<BackupRecordDto> { Page = 3, PageSize = 10 };
        _service.GetPagedAsync(3, 10, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetPaged(3, 10, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetPaged_Defaults_AreFirstPageOfTwenty()
    {
        await CreateController().GetPaged(cancellationToken: Ct);

        await _service.Received(1).GetPagedAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ReturnsTheRecord()
    {
        var dto = new BackupRecordDto { Id = Guid.NewGuid() };
        _service.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetById(dto.Id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_Unknown_PropagatesNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<BackupRecordDto>(_ => throw new NotFoundException("Backup record not found."));

        await Assert.ThrowsAsync<NotFoundException>(() => CreateController().GetById(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Delete_DeletesTheRecordAndReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await CreateController().Delete(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
