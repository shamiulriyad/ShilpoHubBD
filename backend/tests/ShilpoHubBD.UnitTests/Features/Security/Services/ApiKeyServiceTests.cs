using System.Security.Cryptography;
using System.Text;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Security;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Security.Services;

[Trait("Feature", "Security")]
[Trait("Layer", "Service")]
public class ApiKeyServiceTests
{
    private readonly IApiKeyRepository _keys = Substitute.For<IApiKeyRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ApiKeyService CreateService() => new(_keys, _users);

    private ApiKey StoredKey(bool isActive = true)
    {
        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = "Integration",
            KeyPrefix = "shb_12345678",
            KeyHash = "HASH",
            CreatedBy = TestUsers.Create(fullName: "Admin Person"),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
        };
        _keys.GetByIdAsync(key.Id, Arg.Any<CancellationToken>()).Returns(key);
        return key;
    }

    [Fact]
    public async Task CreateAsync_ReturnsAShbPrefixedRawKeyOnceAndStoresOnlyItsHash()
    {
        var admin = TestUsers.Create(fullName: "Admin Person");
        _users.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        ApiKey? saved = null;
        await _keys.AddAsync(Arg.Do<ApiKey>(k => saved = k), Arg.Any<CancellationToken>());

        var result = await CreateService().CreateAsync(admin.Id, new CreateApiKeyRequest { Name = "  Reporting  " }, Ct);

        Assert.Matches("^shb_[0-9a-f]{48}$", result.ApiKey);
        Assert.NotNull(saved);
        var expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.ApiKey)));
        Assert.Equal(expectedHash, saved.KeyHash);
        Assert.DoesNotContain(result.ApiKey, new[] { saved.KeyHash, saved.KeyPrefix, saved.Name });
        Assert.Equal(result.ApiKey[..12], saved.KeyPrefix);
        Assert.Equal(saved.KeyPrefix, result.KeyPrefix);
        await _keys.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_StoresTrimmedNameCreatorAndExpiryAsAnActiveKey()
    {
        var admin = TestUsers.Create();
        _users.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        var expiry = DateTime.UtcNow.AddDays(90);
        ApiKey? saved = null;
        await _keys.AddAsync(Arg.Do<ApiKey>(k => saved = k), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var result = await CreateService().CreateAsync(admin.Id, new CreateApiKeyRequest { Name = "  Reporting  ", ExpiresAt = expiry }, Ct);

        Assert.NotNull(saved);
        Assert.Equal("Reporting", saved.Name);
        Assert.Equal(admin.Id, saved.CreatedByUserId);
        Assert.Same(admin, saved.CreatedBy);
        Assert.True(saved.IsActive);
        Assert.Equal(expiry, saved.ExpiresAt);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(saved.Id, result.Id);
        Assert.Equal("Reporting", result.Name);
        Assert.Equal(expiry, result.ExpiresAt);
        Assert.Equal(saved.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_TwoKeys_AreDifferent()
    {
        var admin = TestUsers.Create();
        _users.GetByIdAsync(admin.Id, Arg.Any<CancellationToken>()).Returns(admin);
        var service = CreateService();

        var first = await service.CreateAsync(admin.Id, new CreateApiKeyRequest { Name = "A" }, Ct);
        var second = await service.CreateAsync(admin.Id, new CreateApiKeyRequest { Name = "B" }, Ct);

        Assert.NotEqual(first.ApiKey, second.ApiKey);
    }

    [Fact]
    public async Task CreateAsync_UnknownUser_ThrowsNotFoundAndStoresNothing()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().CreateAsync(Guid.NewGuid(), new CreateApiKeyRequest { Name = "A" }, Ct));

        Assert.Equal("User not found.", error.Message);
        await _keys.DidNotReceive().AddAsync(Arg.Any<ApiKey>(), Arg.Any<CancellationToken>());
        await _keys.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-3, 10, 1, 10)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 101, 2, 20)]
    [InlineData(3, 100, 3, 100)]
    [InlineData(1, 1, 1, 1)]
    public async Task GetPagedAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _keys.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<ApiKey>(), 0));

        var result = await CreateService().GetPagedAsync(page, pageSize, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
        await _keys.Received(1).GetPagedAsync(expectedPage, expectedSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_MapsKeysWithTheCreatorsNameAndTotalCount()
    {
        var key = StoredKey();
        key.LastUsedAt = DateTime.UtcNow.AddHours(-1);
        _keys.GetPagedAsync(1, 20, Arg.Any<CancellationToken>()).Returns((new List<ApiKey> { key }, 41));

        var result = await CreateService().GetPagedAsync(1, 20, Ct);

        Assert.Equal(41, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(key.Id, dto.Id);
        Assert.Equal("Integration", dto.Name);
        Assert.Equal("shb_12345678", dto.KeyPrefix);
        Assert.Equal("Admin Person", dto.CreatedByName);
        Assert.True(dto.IsActive);
        Assert.Equal(key.LastUsedAt, dto.LastUsedAt);
        Assert.Equal(key.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public async Task RevokeAsync_ActiveKey_DeactivatesItAndRecordsWhen()
    {
        var key = StoredKey();
        var before = DateTime.UtcNow;

        var dto = await CreateService().RevokeAsync(key.Id, Ct);

        Assert.False(key.IsActive);
        Assert.NotNull(key.RevokedAt);
        Assert.InRange(key.RevokedAt.Value, before, DateTime.UtcNow);
        Assert.False(dto.IsActive);
        Assert.Equal(key.RevokedAt, dto.RevokedAt);
        await _keys.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeAsync_AlreadyRevokedKey_ThrowsConflictAndSavesNothing()
    {
        var key = StoredKey(isActive: false);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().RevokeAsync(key.Id, Ct));

        Assert.Equal("API key is already revoked.", error.Message);
        await _keys.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeAsync_UnknownKey_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().RevokeAsync(Guid.NewGuid(), Ct));

        Assert.Equal("API key not found.", error.Message);
    }
}
