using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.QRVerification;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "QRVerification")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class QRVerificationRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public QRVerificationRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<Product> SeedProductAsync(ShilpoHubDbContext context)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var producer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = Guid.NewGuid().ToString("N"), Price = 500, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Categories.Add(category);
        context.Users.Add(producer);
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        return product;
    }

    private static QRCode MakeQrCode(Product product, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(), ProductId = product.Id, Code = Guid.NewGuid().ToString("N"), IsActive = isActive, CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new QRVerificationRepository(scope.NewContext());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItWithProductProducerAndDistrictLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        seedContext.QRCodes.Add(qrCode);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var found = await repository.GetByIdAsync(qrCode.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(product.Name, found!.Product.Name);
        Assert.NotNull(found.Product.Producer);
        Assert.NotNull(found.Product.District);
    }

    [Fact]
    public async Task GetByCodeAsync_UnknownCode_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new QRVerificationRepository(scope.NewContext());

        Assert.Null(await repository.GetByCodeAsync("missing", Ct));
    }

    [Fact]
    public async Task GetByCodeAsync_KnownCode_ReturnsIt()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        seedContext.QRCodes.Add(qrCode);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var found = await repository.GetByCodeAsync(qrCode.Code, Ct);

        Assert.NotNull(found);
        Assert.Equal(qrCode.Id, found!.Id);
    }

    [Fact]
    public async Task GetActiveByProductIdAsync_OnlyRevokedCodeExists_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        seedContext.QRCodes.Add(MakeQrCode(product, isActive: false));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());

        Assert.Null(await repository.GetActiveByProductIdAsync(product.Id, Ct));
    }

    [Fact]
    public async Task GetActiveByProductIdAsync_ActiveCodeExists_ReturnsIt()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        seedContext.QRCodes.Add(qrCode);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var found = await repository.GetActiveByProductIdAsync(product.Id, Ct);

        Assert.Equal(qrCode.Id, found!.Id);
    }

    [Fact]
    public async Task AddQRCodeAsync_SameCodeTwice_ViolatesTheUniqueIndex()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var code = Guid.NewGuid().ToString("N");
        seedContext.QRCodes.Add(new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Code = code, CreatedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        await repository.AddQRCodeAsync(new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Code = code, CreatedAt = DateTime.UtcNow }, Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task AddVerificationRecordAsync_ThenSaveChangesAsync_PersistsTheRecord()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        var user = TestUsers.Create();
        seedContext.QRCodes.Add(qrCode);
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var record = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = qrCode.Code, QRCodeId = qrCode.Id, VerifiedByUserId = user.Id, IsValid = true, VerifiedAt = DateTime.UtcNow };
        await repository.AddVerificationRecordAsync(record, Ct);
        await repository.SaveChangesAsync(Ct);

        var (items, totalCount) = await new QRVerificationRepository(scope.NewContext()).GetHistoryForUserAsync(user.Id, 1, 20, Ct);
        Assert.Equal(1, totalCount);
        Assert.Equal(record.Id, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetHistoryForUserAsync_ReturnsOnlyThatUsersRecords_NewestFirstAndPaged()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        var user = TestUsers.Create();
        var other = TestUsers.Create();
        seedContext.QRCodes.Add(qrCode);
        seedContext.Users.AddRange(user, other);
        var older = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = qrCode.Code, QRCodeId = qrCode.Id, VerifiedByUserId = user.Id, IsValid = true, VerifiedAt = DateTime.UtcNow.AddMinutes(-5) };
        var newer = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = qrCode.Code, QRCodeId = qrCode.Id, VerifiedByUserId = user.Id, IsValid = true, VerifiedAt = DateTime.UtcNow };
        var somebodyElses = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = qrCode.Code, QRCodeId = qrCode.Id, VerifiedByUserId = other.Id, IsValid = true, VerifiedAt = DateTime.UtcNow };
        seedContext.QRVerificationRecords.AddRange(older, newer, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var (items, totalCount) = await repository.GetHistoryForUserAsync(user.Id, 1, 20, Ct);

        Assert.Equal(2, totalCount);
        Assert.Equal(new[] { newer.Id, older.Id }, items.Select(r => r.Id));
    }

    [Fact]
    public async Task GetHistoryForProductAsync_ReturnsOnlyThatProductsRecords()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var product = await SeedProductAsync(seedContext);
        var otherProduct = await SeedProductAsync(seedContext);
        var qrCode = MakeQrCode(product);
        var otherQrCode = MakeQrCode(otherProduct);
        var user = TestUsers.Create();
        seedContext.QRCodes.AddRange(qrCode, otherQrCode);
        seedContext.Users.Add(user);
        var mine = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = qrCode.Code, QRCodeId = qrCode.Id, VerifiedByUserId = user.Id, IsValid = true, VerifiedAt = DateTime.UtcNow };
        var somebodyElses = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = otherQrCode.Code, QRCodeId = otherQrCode.Id, VerifiedByUserId = user.Id, IsValid = true, VerifiedAt = DateTime.UtcNow };
        seedContext.QRVerificationRecords.AddRange(mine, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new QRVerificationRepository(scope.NewContext());
        var (items, totalCount) = await repository.GetHistoryForProductAsync(product.Id, 1, 20, Ct);

        Assert.Equal(1, totalCount);
        Assert.Equal(mine.Id, Assert.Single(items).Id);
    }
}
