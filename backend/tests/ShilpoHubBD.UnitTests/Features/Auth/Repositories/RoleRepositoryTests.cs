using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Auth.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Auth")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class RoleRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public RoleRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetByNameAsync_SeededRole_ReturnsItWithItsFixedId()
    {
        await using var db = await _database.BeginAsync();

        var role = await new RoleRepository(db.NewContext()).GetByNameAsync(RoleNames.Customer, Ct);

        Assert.NotNull(role);
        Assert.Equal(new Guid("00000000-0000-0000-0000-000000000001"), role.Id);
    }

    [Theory]
    [InlineData("Ghost")]
    [InlineData("customer")]
    public async Task GetByNameAsync_UnknownOrWrongCaseName_ReturnsNull(string name)
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new RoleRepository(db.NewContext()).GetByNameAsync(name, Ct));
    }

    [Fact]
    public async Task GetByNameAsync_RoleAddedLater_IsFound()
    {
        await using var db = await _database.BeginAsync();
        var name = TestUsers.UniqueRoleName();
        var context = db.NewContext();
        context.Roles.Add(TestUsers.Role(name));
        await context.SaveChangesAsync(Ct);

        Assert.NotNull(await new RoleRepository(db.NewContext()).GetByNameAsync(name, Ct));
    }

    [Fact]
    public async Task GetByNamesAsync_ReturnsOnlyTheRolesThatExist()
    {
        await using var db = await _database.BeginAsync();

        var roles = await new RoleRepository(db.NewContext())
            .GetByNamesAsync(new[] { RoleNames.Producer, RoleNames.Tourist, "Ghost" }, Ct);

        Assert.Equal(new[] { RoleNames.Producer, RoleNames.Tourist }, roles.Select(r => r.Name).Order());
    }

    [Fact]
    public async Task GetByNamesAsync_NoNames_ReturnsEmpty()
    {
        await using var db = await _database.BeginAsync();

        Assert.Empty(await new RoleRepository(db.NewContext()).GetByNamesAsync(Array.Empty<string>(), Ct));
    }
}
