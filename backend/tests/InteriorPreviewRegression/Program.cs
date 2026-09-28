// Regression checks for the real Interior Preview feature (GeminiInteriorPreviewProvider): a valid
// product + room photo produces a genuinely new stored file (never a placeholder, never the same file
// twice), an invalid product/image/config/AI-failure/storage-failure each raise a clear typed
// exception instead of ever returning a static or fabricated image. Uses a real temp directory for
// file I/O (this feature's whole job is storage), a stub HttpMessageHandler standing in for Gemini,
// and hand-rolled fakes for everything else. Run with `dotnet run`.
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Infrastructure.AIShopping;
using ShilpoHubBD.Infrastructure.Options;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

// ---- fixture: a temp "wwwroot" with a real stored product image file already on disk ----
var tempRoot = Directory.CreateTempSubdirectory("interior-preview-regression-").FullName;
var imagesFolder = Path.Combine(tempRoot, "uploads", "images");
Directory.CreateDirectory(imagesFolder);
var storedProductImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
File.WriteAllBytes(Path.Combine(imagesFolder, "product.jpg"), storedProductImageBytes);

var producer = new User { Id = Guid.NewGuid(), FullName = "Test Producer", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var district = new District { Id = Guid.NewGuid(), Name = "Dhaka", Division = "Dhaka" };
var category = new Category { Id = Guid.NewGuid(), Name = "Pottery & Terracotta", Slug = "pottery-terracotta" };

var product = new Product
{
    Id = Guid.NewGuid(), Name = "Terracotta Vase", Slug = "terracotta-vase", Description = "A vase",
    Price = 500m, CategoryId = category.Id, Category = category, DistrictId = district.Id, District = district,
    ProducerId = producer.Id, Producer = producer,
};
product.Images.Add(new ProductImage { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, ImageUrl = "/uploads/images/product.jpg", IsPrimary = true });

var productWithNoImage = new Product
{
    Id = Guid.NewGuid(), Name = "No Image Product", Slug = "no-image-product", Description = "x",
    Price = 100m, CategoryId = category.Id, Category = category, DistrictId = district.Id, District = district,
    ProducerId = producer.Id, Producer = producer,
};

var geminiOptions = Options.Create(new GeminiOptions { ApiKey = "test-key", Model = "gemini-test", ImageModel = "gemini-image-test" });
var storageOptions = Options.Create(new ImageStorageOptions { WebRootPath = tempRoot });
var roomImageBytes = new byte[] { 1, 2, 3, 4, 5 };

string GeminiImageSuccessJson(byte[] bytes) => JsonSerializer.Serialize(new
{
    candidates = new[]
    {
        new { content = new { parts = new[] { new { inlineData = new { mimeType = "image/png", data = Convert.ToBase64String(bytes) } } } } },
    },
});

HttpClient MakeClient(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
    new(new StubHandler(responder)) { BaseAddress = new Uri("https://gemini.invalid/v1beta/") };

GeminiInteriorPreviewProvider MakeProvider(HttpClient httpClient, Product? repoProduct, IOptions<ImageStorageOptions>? storage = null) => new(
    httpClient, geminiOptions, storage ?? storageOptions, new FakeProductRepository(repoProduct), NullLogger<GeminiInteriorPreviewProvider>.Instance);

// ===================== Valid product + valid image =====================
var generatedImageBytes = new byte[] { 9, 9, 9, 9 };
var successClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent(GeminiImageSuccessJson(generatedImageBytes), Encoding.UTF8, "application/json"),
});
var successProvider = MakeProvider(successClient, product);
var successResult = await successProvider.GetPreviewAsync(
    new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
    CancellationToken.None);

Check("Valid product + valid image: returns a real generated preview URL, not a placeholder",
    successResult.PreviewImageUrl.StartsWith("/uploads/images/") && !successResult.PreviewImageUrl.Contains("placehold"));
Check("Valid product + valid image: the generated file was actually written to storage",
    File.Exists(Path.Combine(tempRoot, successResult.PreviewImageUrl.TrimStart('/'))));
Check("Valid product + valid image: the saved file's bytes are the ones the AI provider returned, not fabricated",
    File.ReadAllBytes(Path.Combine(tempRoot, successResult.PreviewImageUrl.TrimStart('/'))).SequenceEqual(generatedImageBytes));
Check("Valid product + valid image: description names the real product", successResult.Description.Contains(product.Name));

// ===================== Invalid product =====================
var threwNotFound = false;
try
{
    await MakeProvider(successClient, null).GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = Guid.NewGuid(), RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (NotFoundException) { threwNotFound = true; }
Check("Invalid product: raises NotFoundException (404), not a placeholder image", threwNotFound);

// ===================== Invalid image (empty) =====================
var threwValidationEmpty = false;
try
{
    await successProvider.GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = Array.Empty<byte>(), RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (FluentValidation.ValidationException) { threwValidationEmpty = true; }
Check("Invalid image (empty): raises a validation error, not a placeholder image", threwValidationEmpty);

// ===================== Unsupported image format =====================
var threwValidationFormat = false;
try
{
    await successProvider.GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "application/pdf" },
        CancellationToken.None);
}
catch (FluentValidation.ValidationException) { threwValidationFormat = true; }
Check("Unsupported image format: raises a validation error, not a placeholder image", threwValidationFormat);

// ===================== AI API failure =====================
var failingClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
var threwAiFailure = false;
try
{
    await MakeProvider(failingClient, product).GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (AiServiceUnavailableException) { threwAiFailure = true; }
Check("AI API failure: raises AiServiceUnavailableException (503), never a placeholder image", threwAiFailure);

// ===================== Missing API configuration =====================
var unconfiguredOptions = Options.Create(new GeminiOptions { ApiKey = "", Model = "gemini-test", ImageModel = "" });
var unconfiguredProvider = new GeminiInteriorPreviewProvider(
    new HttpClient(), unconfiguredOptions, storageOptions, new FakeProductRepository(product), NullLogger<GeminiInteriorPreviewProvider>.Instance);
var threwUnconfigured = false;
try
{
    await unconfiguredProvider.GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (AiServiceUnavailableException) { threwUnconfigured = true; }
Check("Missing API configuration: raises AiServiceUnavailableException, never a placeholder image", threwUnconfigured);

// ===================== Product exists but has no stored image =====================
var threwConflict = false;
try
{
    await MakeProvider(successClient, productWithNoImage).GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = productWithNoImage.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (ConflictException) { threwConflict = true; }
Check("Product with no stored image: raises ConflictException rather than fabricating a product photo", threwConflict);

// ===================== Storage failure =====================
var badStorageOptions = Options.Create(new ImageStorageOptions { WebRootPath = Path.Combine(tempRoot, "bad\0folder") });
var threwStorageFailure = false;
try
{
    await MakeProvider(successClient, product, badStorageOptions).GetPreviewAsync(
        new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
        CancellationToken.None);
}
catch (AiServiceUnavailableException) { threwStorageFailure = true; }
Check("Storage failure: raises AiServiceUnavailableException rather than an unhandled crash or a placeholder", threwStorageFailure);

// ===================== Never the same image twice =====================
var secondClient = MakeClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent(GeminiImageSuccessJson(new byte[] { 7, 7, 7 }), Encoding.UTF8, "application/json"),
});
var secondResult = await MakeProvider(secondClient, product).GetPreviewAsync(
    new InteriorPreviewRequest { ProductId = product.Id, RoomImageBytes = roomImageBytes, RoomImageContentType = "image/jpeg" },
    CancellationToken.None);
Check("Two requests for the same product get distinct generated files, never a cached/static image",
    secondResult.PreviewImageUrl != successResult.PreviewImageUrl);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
Directory.Delete(tempRoot, recursive: true);
return failures == 0 ? 0 : 1;

class StubHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
    public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_responder(request));
}

class FakeProductRepository : IProductRepository
{
    private readonly Product? _product;
    public FakeProductRepository(Product? product) => _product = product;

    public Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_product is not null && _product.Id == id ? _product : null);
    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) => Task.FromResult<Product?>(null);
    public Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<List<Product>> GetTrendingAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<(List<Product> Items, int TotalCount)> GetPendingApprovalAsync(int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<(decimal AveragePrice, int SampleSize)> GetCategoryPriceStatsAsync(Guid categoryId, CancellationToken ct) => Task.FromResult((0m, 0));
    public Task<List<Product>> GetByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct) => Task.FromResult(false);
    public Task<List<Product>> GetLowStockByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task AddAsync(Product product, CancellationToken ct) => Task.CompletedTask;
    public Task AddVariantAsync(ProductVariant variant, CancellationToken ct) => Task.CompletedTask;
    public Task AddVideoAsync(ProductVideo video, CancellationToken ct) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
