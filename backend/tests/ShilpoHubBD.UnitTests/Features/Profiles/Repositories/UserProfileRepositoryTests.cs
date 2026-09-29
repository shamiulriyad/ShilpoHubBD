using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Profiles.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Profiles")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class UserProfileRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public UserProfileRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // District.Name has a unique index and the 64 real districts (including "Dhaka") are already
    // seeded by an EF migration, so tests need their own distinct, made-up name.
    private static District MakeDistrict(string? name = null) => new() { Id = Guid.NewGuid(), Name = name ?? $"Test District {Guid.NewGuid():N}", Division = "Dhaka" };

    private static UserProfile Profile(User user, UserProfileStatus status = UserProfileStatus.Pending, string legalName = "Rahima Begum",
        string nid = "1234567890", District? district = null, string? expertise = null) => new()
    {
        Id = Guid.NewGuid(), UserId = user.Id, LegalName = legalName, Phone = "+8801700000000", NidNumber = nid,
        AddressLine = "House 1", Status = status, DistrictId = district?.Id, District = district, Expertise = expertise,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    // ---------- AddAsync / GetByUserIdAsync / GetByIdAsync ----------

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheProfile()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var user = TestUsers.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync(Ct);
        var profile = Profile(user);
        var repository = new UserProfileRepository(db.NewContext());

        await repository.AddAsync(profile, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().UserProfiles.AnyAsync(p => p.Id == profile.Id, Ct));
    }

    [Fact]
    public async Task GetByUserIdAsync_LoadsTheUserAndDistrict()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var user = TestUsers.Create(fullName: "Rahima Begum");
        var district = MakeDistrict();
        context.Users.Add(user);
        context.Districts.Add(district);
        context.UserProfiles.Add(Profile(user, district: district));
        await context.SaveChangesAsync(Ct);

        var found = await new UserProfileRepository(db.NewContext()).GetByUserIdAsync(user.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal("Rahima Begum", found.User.FullName);
        Assert.Equal(district.Name, found.District!.Name);
    }

    [Fact]
    public async Task GetByUserIdAsync_NoProfile_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new UserProfileRepository(db.NewContext()).GetByUserIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new UserProfileRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    // ---------- NidInUseAsync ----------

    [Fact]
    public async Task NidInUseAsync_AnotherUsersNid_ReturnsTrue()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var owner = TestUsers.Create();
        context.Users.Add(owner);
        context.UserProfiles.Add(Profile(owner, nid: "1111111111"));
        await context.SaveChangesAsync(Ct);

        Assert.True(await new UserProfileRepository(db.NewContext()).NidInUseAsync("1111111111", Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task NidInUseAsync_TheSameUsersOwnNid_ReturnsFalse()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var owner = TestUsers.Create();
        context.Users.Add(owner);
        context.UserProfiles.Add(Profile(owner, nid: "2222222222"));
        await context.SaveChangesAsync(Ct);

        Assert.False(await new UserProfileRepository(db.NewContext()).NidInUseAsync("2222222222", owner.Id, Ct));
    }

    [Fact]
    public async Task NidInUseAsync_UnusedNid_ReturnsFalse()
    {
        await using var db = await _database.BeginAsync();

        Assert.False(await new UserProfileRepository(db.NewContext()).NidInUseAsync("9999999999", Guid.NewGuid(), Ct));
    }

    // ---------- GetProducerExpertiseOptionsAsync ----------

    [Fact]
    public async Task GetProducerExpertiseOptionsAsync_ReturnsTrimmedApprovedProducerExpertiseSortedAlphabetically()
    {
        // The method returns every producer's expertise platform-wide, so this test tags its own
        // values and filters the result down to them, rather than asserting the exact full list --
        // other tests' committed rows may coexist when the suite runs in parallel.
        var tag = Guid.NewGuid().ToString("N")[..8];
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Producer, Ct);
        var weaver = TestUsers.Create().WithRoles(producerRole);
        var potter = TestUsers.Create().WithRoles(producerRole);
        var customer = TestUsers.Create().WithRoles(await context.Roles.SingleAsync(r => r.Name == RoleNames.Customer, Ct));
        context.Users.AddRange(weaver, potter, customer);
        context.UserProfiles.AddRange(
            Profile(weaver, UserProfileStatus.Approved, expertise: $"  Jamdani Weaving {tag}  "),
            Profile(potter, UserProfileStatus.Approved, expertise: $"Pottery {tag}", nid: "3333333333"),
            Profile(customer, UserProfileStatus.Approved, expertise: $"Should not appear {tag} (not a producer)", nid: "5555555555"));
        await context.SaveChangesAsync(Ct);

        var options = await new UserProfileRepository(db.NewContext()).GetProducerExpertiseOptionsAsync(Ct);
        var ours = options.Where(o => o.Contains(tag, StringComparison.Ordinal)).ToList();

        Assert.Equal(new[] { $"Jamdani Weaving {tag}", $"Pottery {tag}" }, ours);
    }

    [Fact]
    public async Task GetProducerExpertiseOptionsAsync_TwoApprovedProducersWithTheSameExpertiseInDifferentCase_CollapseToOneEntry()
    {
        // GetProducerExpertiseOptionsAsync applies Distinct(StringComparer.OrdinalIgnoreCase) to rows
        // read with no ORDER BY, so which casing survives is whatever order Postgres happens to return
        // them in -- not something this test (or any caller) can rely on. Only the dedup itself is
        // guaranteed, which is what this test checks.
        var tag = Guid.NewGuid().ToString("N")[..8];
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Producer, Ct);
        var weaver = TestUsers.Create().WithRoles(producerRole);
        var duplicateWeaver = TestUsers.Create().WithRoles(producerRole);
        context.Users.AddRange(weaver, duplicateWeaver);
        context.UserProfiles.AddRange(
            Profile(weaver, UserProfileStatus.Approved, expertise: $"Jamdani Weaving {tag}"),
            Profile(duplicateWeaver, UserProfileStatus.Approved, expertise: $"jamdani weaving {tag}", nid: "4444444444"));
        await context.SaveChangesAsync(Ct);

        var options = await new UserProfileRepository(db.NewContext()).GetProducerExpertiseOptionsAsync(Ct);
        var ours = options.Where(o => o.Contains(tag, StringComparison.Ordinal)).ToList();

        var only = Assert.Single(ours);
        Assert.Equal($"jamdani weaving {tag}", only, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProducerExpertiseOptionsAsync_PendingProducerProfile_IsExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Producer, Ct);
        var producer = TestUsers.Create().WithRoles(producerRole);
        context.Users.Add(producer);
        context.UserProfiles.Add(Profile(producer, UserProfileStatus.Pending, expertise: "Should not appear"));
        await context.SaveChangesAsync(Ct);

        var options = await new UserProfileRepository(db.NewContext()).GetProducerExpertiseOptionsAsync(Ct);

        Assert.DoesNotContain("Should not appear", options);
    }

    // ---------- IsApprovedAsync ----------

    [Fact]
    public async Task IsApprovedAsync_ReflectsTheProfilesStatus()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var approved = TestUsers.Create();
        var pending = TestUsers.Create();
        context.Users.AddRange(approved, pending);
        context.UserProfiles.AddRange(
            Profile(approved, UserProfileStatus.Approved, nid: "6666666666"),
            Profile(pending, UserProfileStatus.Pending, nid: "7777777777"));
        await context.SaveChangesAsync(Ct);
        var repository = new UserProfileRepository(db.NewContext());

        Assert.True(await repository.IsApprovedAsync(approved.Id, Ct));
        Assert.False(await repository.IsApprovedAsync(pending.Id, Ct));
        Assert.False(await repository.IsApprovedAsync(Guid.NewGuid(), Ct));
    }

    // ---------- GetPagedAsync ----------

    [Fact]
    public async Task GetPagedAsync_OrdersByStatusTextAlphabeticallyThenNewestUpdatedWithinEachStatus()
    {
        // Status is stored as its string name (HasConversion<string>()), and OrderBy(p => p.Status)
        // is translated to an ORDER BY on that text column, so it sorts alphabetically -- "Approved"
        // before "Pending" before "Rejected" -- not by review urgency. This looks like it could be an
        // unintended consequence of the string conversion rather than a deliberate design choice.
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var now = DateTime.UtcNow;
        var older = TestUsers.Create(fullName: "Older Pending");
        var newer = TestUsers.Create(fullName: "Newer Pending");
        var approved = TestUsers.Create(fullName: "Approved One");
        context.Users.AddRange(older, newer, approved);
        var olderProfile = Profile(older, UserProfileStatus.Pending, nid: "8000000001");
        olderProfile.UpdatedAt = now.AddDays(-1);
        var newerProfile = Profile(newer, UserProfileStatus.Pending, nid: "8000000002");
        newerProfile.UpdatedAt = now;
        var approvedProfile = Profile(approved, UserProfileStatus.Approved, nid: "8000000003");
        approvedProfile.UpdatedAt = now.AddDays(1);
        context.UserProfiles.AddRange(olderProfile, newerProfile, approvedProfile);
        await context.SaveChangesAsync(Ct);

        // Scoped to these three rows by NID prefix, so the order isn't disturbed by whatever else the
        // shared test database happens to hold.
        var (items, total) = await new UserProfileRepository(db.NewContext())
            .GetPagedAsync(new UserProfileQueryParameters { Search = "800000000", PageSize = 10 }, Ct);

        Assert.Equal(3, total);
        Assert.Equal(new[] { "8000000003", "8000000002", "8000000001" }, items.Select(p => p.NidNumber));
    }

    [Fact]
    public async Task GetPagedAsync_ByStatus_ReturnsOnlyThatStatus()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var pending = TestUsers.Create();
        var rejected = TestUsers.Create();
        context.Users.AddRange(pending, rejected);
        context.UserProfiles.AddRange(
            Profile(pending, UserProfileStatus.Pending, nid: "9000000001"),
            Profile(rejected, UserProfileStatus.Rejected, nid: "9000000002"));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new UserProfileRepository(db.NewContext())
            .GetPagedAsync(new UserProfileQueryParameters { Status = UserProfileStatus.Rejected }, Ct);

        Assert.Equal(1, total);
        Assert.Equal("9000000002", Assert.Single(items).NidNumber);
    }

    [Fact]
    public async Task GetPagedAsync_Search_MatchesLegalNameNidOrEmailCaseInsensitively()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var target = TestUsers.Create("findme@example.com");
        context.Users.Add(target);
        context.UserProfiles.Add(Profile(target, legalName: "Findable Weaver", nid: "9100000001"));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new UserProfileRepository(db.NewContext())
            .GetPagedAsync(new UserProfileQueryParameters { Search = "findable" }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(target.Id, Assert.Single(items).UserId);
    }

    // ---------- GetRolesAsync ----------

    [Fact]
    public async Task GetRolesAsync_ReturnsEachRequestedUsersRoles()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var producerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Producer, Ct);
        var customerRole = await context.Roles.SingleAsync(r => r.Name == RoleNames.Customer, Ct);
        var producer = TestUsers.Create().WithRoles(producerRole);
        var customer = TestUsers.Create().WithRoles(customerRole);
        context.Users.AddRange(producer, customer);
        await context.SaveChangesAsync(Ct);

        var roles = await new UserProfileRepository(db.NewContext()).GetRolesAsync(new[] { producer.Id, customer.Id }, Ct);

        Assert.Equal(RoleNames.Producer, Assert.Single(roles[producer.Id]));
        Assert.Equal(RoleNames.Customer, Assert.Single(roles[customer.Id]));
    }

    [Fact]
    public async Task GetRolesAsync_UnknownUserId_IsNotInTheResult()
    {
        await using var db = await _database.BeginAsync();

        var roles = await new UserProfileRepository(db.NewContext()).GetRolesAsync(new[] { Guid.NewGuid() }, Ct);

        Assert.Empty(roles);
    }

    // ---------- DistrictExistsAsync ----------

    [Fact]
    public async Task DistrictExistsAsync_ReflectsWhetherTheDistrictRowExists()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var district = MakeDistrict();
        context.Districts.Add(district);
        await context.SaveChangesAsync(Ct);
        var repository = new UserProfileRepository(db.NewContext());

        Assert.True(await repository.DistrictExistsAsync(district.Id, Ct));
        Assert.False(await repository.DistrictExistsAsync(Guid.NewGuid(), Ct));
    }
}
