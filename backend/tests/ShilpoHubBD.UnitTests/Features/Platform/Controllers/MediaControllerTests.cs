using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Platform.Controllers;

[Trait("Feature", "Platform")]
[Trait("Layer", "Controller")]
public sealed class MediaControllerTests : IDisposable
{
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "shilpohub-media-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_webRoot))
        {
            Directory.Delete(_webRoot, recursive: true);
        }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private MediaController CreateController()
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.WebRootPath.Returns(_webRoot);
        environment.WebRootFileProvider.Returns(new NullFileProvider());
        return new MediaController(environment).WithUser(Guid.NewGuid(), RoleNames.Producer);
    }

    private static IFormFile MakeFile(string contentType, int length = 100, string fileName = "photo.jpg")
    {
        var stream = new MemoryStream(new byte[length]);
        return new FormFile(stream, 0, length, "file", fileName) { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    [Fact]
    public void Controller_IsRestrictedToProducerAndSuperAdminUnderApiMedia()
    {
        Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}", AccessRules.ClassRoles(typeof(MediaController)));
        Assert.Equal("api/media", AccessRules.ControllerRoute(typeof(MediaController)));
    }

    [Fact]
    public void UploadImage_IsAPostOnImages()
        => Assert.Equal(("POST", "images"), AccessRules.ActionRoute(typeof(MediaController), nameof(MediaController.UploadImage)));

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    public async Task UploadImage_AllowedContentType_SavesTheFileAndReturnsItsUrl(string contentType, string extension)
    {
        var result = await CreateController().UploadImage(MakeFile(contentType), Ct);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var url = ok.Value!.GetType().GetProperty("url")!.GetValue(ok.Value) as string;
        Assert.NotNull(url);
        Assert.StartsWith("/uploads/images/", url);
        Assert.EndsWith(extension, url);
        Assert.True(File.Exists(Path.Combine(_webRoot, url!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task UploadImage_UnknownContentType_ReturnsBadRequestAndSavesNothing()
    {
        var result = await CreateController().UploadImage(MakeFile("application/pdf"), Ct);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only JPG, PNG and WebP images are supported.",
            bad.Value!.GetType().GetProperty("message")!.GetValue(bad.Value));
        Assert.False(Directory.Exists(Path.Combine(_webRoot, "uploads")));
    }

    [Fact]
    public async Task UploadImage_EmptyFile_ReturnsBadRequest()
    {
        var result = await CreateController().UploadImage(MakeFile("image/jpeg", length: 0), Ct);

        Assert.Equal("Choose an image smaller than 20 MB.",
            Assert.IsType<BadRequestObjectResult>(result.Result).Value!.GetType().GetProperty("message")!
                .GetValue(Assert.IsType<BadRequestObjectResult>(result.Result).Value));
    }

    [Fact]
    public async Task UploadImage_OverTwentyMegabytes_ReturnsBadRequest()
    {
        var result = await CreateController().UploadImage(MakeFile("image/jpeg", length: 20 * 1024 * 1024 + 1), Ct);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UploadImage_TwoUploads_GetDifferentFileNames()
    {
        var controller = CreateController();

        var first = Assert.IsType<OkObjectResult>((await controller.UploadImage(MakeFile("image/jpeg"), Ct)).Result);
        var second = Assert.IsType<OkObjectResult>((await controller.UploadImage(MakeFile("image/jpeg"), Ct)).Result);

        var firstUrl = first.Value!.GetType().GetProperty("url")!.GetValue(first.Value);
        var secondUrl = second.Value!.GetType().GetProperty("url")!.GetValue(second.Value);
        Assert.NotEqual(firstUrl, secondUrl);
    }

    [Fact]
    public async Task UploadImage_NoWebRootPathConfigured_FallsBackToContentRootWwwroot()
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.WebRootPath.Returns((string?)null);
        environment.ContentRootPath.Returns(_webRoot);
        var controller = new MediaController(environment).WithUser(Guid.NewGuid(), RoleNames.Producer);

        var result = await controller.UploadImage(MakeFile("image/jpeg"), Ct);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(Directory.Exists(Path.Combine(_webRoot, "wwwroot", "uploads", "images")));
    }
}
