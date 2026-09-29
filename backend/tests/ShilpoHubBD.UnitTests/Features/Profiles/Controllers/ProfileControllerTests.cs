using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Profiles.Controllers;

[Trait("Feature", "Profiles")]
[Trait("Layer", "Controller")]
public sealed class ProfileControllerTests : IDisposable
{
    private readonly IUserProfileService _service = Substitute.For<IUserProfileService>();
    private readonly IUserProfileRepository _repository = Substitute.For<IUserProfileRepository>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "shilpohub-profile-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_webRoot))
        {
            Directory.Delete(_webRoot, recursive: true);
        }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ProfileController CreateController() => new ProfileController(_service, _repository).WithUser(_userId);

    private IWebHostEnvironment MakeEnvironment()
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.WebRootPath.Returns(_webRoot);
        return environment;
    }

    private static IFormFile MakeFile(string contentType = "image/jpeg", int length = 100)
    {
        var stream = new MemoryStream(new byte[length]);
        return new FormFile(stream, 0, length, "file", "photo.jpg") { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    // ---------- access rules and routes ----------

    [Fact]
    public void Controller_RequiresSignInAtClassLevelWithNoRoutePrefix()
    {
        Assert.True(AccessRules.ClassRequiresSignIn(typeof(ProfileController)));
        Assert.Null(AccessRules.ControllerRoute(typeof(ProfileController)));
    }

    [Fact]
    public void ExpertiseOptions_AllowsAnonymousAccess()
        => Assert.True(AccessRules.ActionAllowsAnonymous(typeof(ProfileController), nameof(ProfileController.ExpertiseOptions)));

    [Theory]
    [InlineData(nameof(ProfileController.GetForAdmin))]
    [InlineData(nameof(ProfileController.Approve))]
    [InlineData(nameof(ProfileController.Reject))]
    public void AdminActions_AreRestrictedToSuperAdmin(string action)
        => Assert.Equal(RoleNames.SuperAdmin,
            typeof(ProfileController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles);

    [Theory]
    [InlineData(nameof(ProfileController.ExpertiseOptions), "GET", "api/profile/expertise-options")]
    [InlineData(nameof(ProfileController.GetMine), "GET", "api/profile/me")]
    [InlineData(nameof(ProfileController.UploadPhoto), "POST", "api/profile/photo")]
    [InlineData(nameof(ProfileController.RemovePhoto), "DELETE", "api/profile/photo")]
    [InlineData(nameof(ProfileController.UpsertMine), "PUT", "api/profile/me")]
    [InlineData(nameof(ProfileController.GetForAdmin), "GET", "api/admin/profiles")]
    [InlineData(nameof(ProfileController.Approve), "POST", "api/admin/profiles/{id:guid}/approve")]
    [InlineData(nameof(ProfileController.Reject), "POST", "api/admin/profiles/{id:guid}/reject")]
    public void Actions_UseTheirVerbAndFullRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(ProfileController), action));

    // ---------- ExpertiseOptions / GetMine ----------

    [Fact]
    public async Task ExpertiseOptions_ReturnsTheApprovedProducerExpertiseList()
    {
        var options = new List<string> { "Jamdani weaving", "Pottery" };
        _repository.GetProducerExpertiseOptionsAsync(Arg.Any<CancellationToken>()).Returns(options);

        var result = await CreateController().ExpertiseOptions(Ct);

        Assert.Same(options, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetMine_ReturnsTheSignedInUsersProfile()
    {
        var dto = new UserProfileDto { LoginEmail = "artisan@example.com" };
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetMine(Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    // ---------- UploadPhoto ----------

    [Fact]
    public async Task UploadPhoto_ValidImage_SavesItAndUpdatesTheProfile()
    {
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(new UserProfileDto { PhotoUrl = null });
        var updated = new UserProfileDto { PhotoUrl = "/uploads/avatars/new.jpg" };
        _service.SetPhotoAsync(_userId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(updated);

        var result = await CreateController().UploadPhoto(MakeFile(), MakeEnvironment(), Ct);

        Assert.Same(updated, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.True(Directory.Exists(Path.Combine(_webRoot, "uploads", "avatars")));
        await _service.Received(1).SetPhotoAsync(_userId, Arg.Is<string>(u => u.StartsWith("/uploads/avatars/", StringComparison.Ordinal)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadPhoto_ReplacingAnExistingAvatar_DeletesTheOldFile()
    {
        var environment = MakeEnvironment();
        Directory.CreateDirectory(Path.Combine(_webRoot, "uploads", "avatars"));
        var oldFile = Path.Combine(_webRoot, "uploads", "avatars", "old.jpg");
        await File.WriteAllTextAsync(oldFile, "old", Ct);
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(new UserProfileDto { PhotoUrl = "/uploads/avatars/old.jpg" });
        _service.SetPhotoAsync(_userId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new UserProfileDto());

        await CreateController().UploadPhoto(MakeFile(), environment, Ct);

        Assert.False(File.Exists(oldFile));
    }

    [Fact]
    public async Task UploadPhoto_EmptyFile_ReturnsBadRequestAndCallsNoService()
    {
        var result = await CreateController().UploadPhoto(MakeFile(length: 0), MakeEnvironment(), Ct);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        await _service.DidNotReceive().SetPhotoAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadPhoto_OverFiveMegabytes_ReturnsBadRequest()
    {
        var result = await CreateController().UploadPhoto(MakeFile(length: 5 * 1024 * 1024 + 1), MakeEnvironment(), Ct);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UploadPhoto_UnsupportedContentType_ReturnsBadRequest()
    {
        var result = await CreateController().UploadPhoto(MakeFile(contentType: "application/pdf"), MakeEnvironment(), Ct);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only JPG, PNG and WebP images are supported.", bad.Value!.GetType().GetProperty("message")!.GetValue(bad.Value));
    }

    // ---------- RemovePhoto ----------

    [Fact]
    public async Task RemovePhoto_ClearsThePhotoAndDeletesTheOldFile()
    {
        var environment = MakeEnvironment();
        Directory.CreateDirectory(Path.Combine(_webRoot, "uploads", "avatars"));
        var file = Path.Combine(_webRoot, "uploads", "avatars", "current.jpg");
        await File.WriteAllTextAsync(file, "x", Ct);
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(new UserProfileDto { PhotoUrl = "/uploads/avatars/current.jpg" });
        var updated = new UserProfileDto { PhotoUrl = null };
        _service.SetPhotoAsync(_userId, null, Arg.Any<CancellationToken>()).Returns(updated);

        var result = await CreateController().RemovePhoto(environment, Ct);

        Assert.Same(updated, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.False(File.Exists(file));
    }

    [Fact]
    public async Task RemovePhoto_NoPhotoToBeginWith_DoesNotTryToDeleteAnything()
    {
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(new UserProfileDto { PhotoUrl = null });
        _service.SetPhotoAsync(_userId, null, Arg.Any<CancellationToken>()).Returns(new UserProfileDto());

        var result = await CreateController().RemovePhoto(MakeEnvironment(), Ct);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task RemovePhoto_UrlPointingOutsideTheAvatarsFolder_IsNeverPassedToFileDelete()
    {
        // Only ever deletes files this feature wrote; a URL with a different prefix is left alone.
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(new UserProfileDto { PhotoUrl = "/uploads/images/product.jpg" });
        _service.SetPhotoAsync(_userId, null, Arg.Any<CancellationToken>()).Returns(new UserProfileDto());
        var otherFile = Path.Combine(_webRoot, "uploads", "images", "product.jpg");
        Directory.CreateDirectory(Path.GetDirectoryName(otherFile)!);
        await File.WriteAllTextAsync(otherFile, "x", Ct);

        await CreateController().RemovePhoto(MakeEnvironment(), Ct);

        Assert.True(File.Exists(otherFile));
    }

    // ---------- UpsertMine ----------

    [Fact]
    public async Task UpsertMine_PassesTheRequestForTheSignedInUser()
    {
        var request = new UpsertUserProfileRequest { LegalName = "Rahima Begum" };
        var dto = new UserProfileDto { LegalName = "Rahima Begum" };
        _service.UpsertMineAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().UpsertMine(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task UpsertMine_ProducerWithoutExpertise_PropagatesTheConflict()
    {
        _service.UpsertMineAsync(Arg.Any<Guid>(), Arg.Any<UpsertUserProfileRequest>(), Arg.Any<CancellationToken>())
            .Returns<UserProfileDto>(_ => throw new ConflictException("Producers must state their expertise (for example Jamdani weaving or pottery)."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().UpsertMine(new UpsertUserProfileRequest(), Ct));
    }

    // ---------- GetForAdmin / Approve / Reject ----------

    [Fact]
    public async Task GetForAdmin_PassesTheQueryThroughAndReturnsOk()
    {
        var query = new UserProfileQueryParameters { Search = "rahima" };
        var page = new PagedResult<UserProfileListItemDto>();
        _service.GetForAdminAsync(query, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().GetForAdmin(query, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Approve_UsesTheSignedInAdminAsReviewer()
    {
        var profileId = Guid.NewGuid();
        var request = new ReviewUserProfileRequest();
        var dto = new UserProfileListItemDto { Id = profileId, Status = "Approved" };
        _service.ApproveAsync(profileId, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Approve(profileId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Reject_UsesTheSignedInAdminAsReviewer()
    {
        var profileId = Guid.NewGuid();
        var request = new ReviewUserProfileRequest { Notes = "Blurry NID photo." };
        var dto = new UserProfileListItemDto { Id = profileId, Status = "Rejected" };
        _service.RejectAsync(profileId, _userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Reject(profileId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Reject_NoReasonGiven_PropagatesTheConflict()
    {
        _service.RejectAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<ReviewUserProfileRequest>(), Arg.Any<CancellationToken>())
            .Returns<UserProfileListItemDto>(_ => throw new ConflictException("Tell the member why the profile was rejected so they can fix it."));

        await Assert.ThrowsAsync<ConflictException>(() => CreateController().Reject(Guid.NewGuid(), new ReviewUserProfileRequest(), Ct));
    }
}
