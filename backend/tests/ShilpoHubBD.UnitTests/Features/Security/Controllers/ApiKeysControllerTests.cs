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
public class ApiKeysControllerTests
{
    private readonly IApiKeyService _service = Substitute.For<IApiKeyService>();
    private readonly Guid _adminId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ApiKeysController CreateController() => new ApiKeysController(_service).WithUser(_adminId, RoleNames.SuperAdmin);

    [Fact]
    public void Controller_IsSuperAdminOnlyUnderTheSecurityApiKeysRoute()
    {
        Assert.Equal(RoleNames.SuperAdmin, AccessRules.ClassRoles(typeof(ApiKeysController)));
        Assert.Equal("api/admin/security/api-keys", AccessRules.ControllerRoute(typeof(ApiKeysController)));
        Assert.All(AccessRules.PublicActions(typeof(ApiKeysController)),
            action => Assert.False(AccessRules.ActionAllowsAnonymous(typeof(ApiKeysController), action)));
    }

    [Theory]
    [InlineData(nameof(ApiKeysController.Create), "POST", null)]
    [InlineData(nameof(ApiKeysController.GetPaged), "GET", null)]
    [InlineData(nameof(ApiKeysController.Revoke), "POST", "{id:guid}/revoke")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(ApiKeysController), action));

    [Fact]
    public async Task Create_CreatesTheKeyForTheSignedInAdminAndReturnsItOnce()
    {
        var request = new CreateApiKeyRequest { Name = "Reporting" };
        var created = new CreateApiKeyResultDto { Id = Guid.NewGuid(), ApiKey = "shb_raw" };
        _service.CreateAsync(_adminId, request, Arg.Any<CancellationToken>()).Returns(created);

        var result = await CreateController().Create(request, Ct);

        Assert.Same(created, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Create_AdminIdentifiedByNameIdentifier_StillUsesTheirId()
    {
        var controller = new ApiKeysController(_service).WithNameIdentifierUser(_adminId, RoleNames.SuperAdmin);

        await controller.Create(new CreateApiKeyRequest { Name = "x" }, Ct);

        await _service.Received(1).CreateAsync(_adminId, Arg.Any<CreateApiKeyRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPaged_PassesPagingThroughAndReturnsOk()
    {
        var page = new PagedResult<ApiKeyDto> { Page = 2, PageSize = 5 };
        _service.GetPagedAsync(2, 5, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetPaged(2, 5, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetPaged_Defaults_AreFirstPageOfTwenty()
    {
        await CreateController().GetPaged(cancellationToken: Ct);

        await _service.Received(1).GetPagedAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_RevokesTheKeyAndReturnsIt()
    {
        var id = Guid.NewGuid();
        var dto = new ApiKeyDto { Id = id, IsActive = false };
        _service.RevokeAsync(id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Revoke(id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Revoke_AlreadyRevoked_PropagatesTheConflict()
    {
        _service.RevokeAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<ApiKeyDto>(_ => throw new ConflictException("API key is already revoked."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().Revoke(Guid.NewGuid(), Ct));
    }
}
