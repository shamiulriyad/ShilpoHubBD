using ShilpoHubBD.Infrastructure.Security;

namespace ShilpoHubBD.UnitTests.Features.Auth.Infrastructure;

[Trait("Feature", "Auth")]
[Trait("Layer", "Infrastructure")]
public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesBcryptHashWithWorkFactor12()
    {
        var hash = _hasher.Hash("Heritage1");

        Assert.StartsWith("$2", hash);
        Assert.Contains("$12$", hash);
        Assert.NotEqual("Heritage1", hash);
    }

    [Fact]
    public void Hash_SamePasswordTwice_UsesADifferentSaltEachTime()
        => Assert.NotEqual(_hasher.Hash("Heritage1"), _hasher.Hash("Heritage1"));

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Heritage1");

        Assert.True(_hasher.Verify("Heritage1", hash));
    }

    [Theory]
    [InlineData("heritage1")]
    [InlineData("Heritage2")]
    [InlineData("")]
    public void Verify_DifferentPassword_ReturnsFalse(string attempt)
    {
        var hash = _hasher.Hash("Heritage1");

        Assert.False(_hasher.Verify(attempt, hash));
    }

    [Fact]
    public void Verify_HashCreatedWithALowerWorkFactor_StillVerifies()
    {
        var existing = BCrypt.Net.BCrypt.HashPassword("Heritage1", workFactor: 4);

        Assert.True(_hasher.Verify("Heritage1", existing));
    }
}
