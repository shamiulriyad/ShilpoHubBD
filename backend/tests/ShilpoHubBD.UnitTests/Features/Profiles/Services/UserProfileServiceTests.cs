using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Profiles;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Profiles.Services;

[Trait("Feature", "Profiles")]
[Trait("Layer", "Service")]
public class UserProfileServiceTests
{
    private readonly IUserProfileRepository _profiles = Substitute.For<IUserProfileRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private UserProfileService CreateService() => new(_profiles, _users);

    private static UpsertUserProfileRequest UpsertRequest() => new()
    {
        LegalName = "  Rahima Begum  ", Phone = "  +8801712345678  ", NidNumber = "  1234567890  ", AddressLine = "  House 12  ",
    };

    // ---------- GetMineAsync ----------

    [Fact]
    public async Task GetMineAsync_NoProfileYet_ReturnsNotSubmittedWithTheLoginEmailAndPhoto()
    {
        var user = TestUsers.Create("artisan@example.com").WithRoles(RoleNames.Customer);
        user.ProfilePhotoUrl = "/uploads/avatars/a.jpg";
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var dto = await CreateService().GetMineAsync(user.Id, Ct);

        Assert.False(dto.Exists);
        Assert.Equal("NotSubmitted", dto.Status);
        Assert.Equal("artisan@example.com", dto.LoginEmail);
        Assert.Equal("/uploads/avatars/a.jpg", dto.PhotoUrl);
        Assert.False(dto.ExpertiseRequired);
    }

    [Fact]
    public async Task GetMineAsync_ProducerWithNoProfile_RequiresExpertise()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        var dto = await CreateService().GetMineAsync(user.Id, Ct);

        Assert.True(dto.ExpertiseRequired);
    }

    [Fact]
    public async Task GetMineAsync_ExistingProfile_ReturnsItsFields()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), LegalName = "Rahima Begum", Status = UserProfileStatus.Approved, NidNumber = "1234567890",
        };
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var dto = await CreateService().GetMineAsync(user.Id, Ct);

        Assert.True(dto.Exists);
        Assert.Equal(profile.Id, dto.Id);
        Assert.Equal("Rahima Begum", dto.LegalName);
        Assert.Equal("Approved", dto.Status);
    }

    [Fact]
    public async Task GetMineAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetMineAsync(Guid.NewGuid(), Ct));

        Assert.Equal("User not found.", error.Message);
    }

    // ---------- SetPhotoAsync ----------

    [Fact]
    public async Task SetPhotoAsync_NewUrl_UpdatesTheUserAndSaves()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        var before = DateTime.UtcNow;

        var dto = await CreateService().SetPhotoAsync(user.Id, "/uploads/avatars/new.jpg", Ct);

        Assert.Equal("/uploads/avatars/new.jpg", user.ProfilePhotoUrl);
        Assert.InRange(user.UpdatedAt, before, DateTime.UtcNow);
        Assert.Equal("/uploads/avatars/new.jpg", dto.PhotoUrl);
        await _users.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPhotoAsync_Null_ClearsThePhoto()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        user.ProfilePhotoUrl = "/uploads/avatars/old.jpg";
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);

        await CreateService().SetPhotoAsync(user.Id, null, Ct);

        Assert.Null(user.ProfilePhotoUrl);
    }

    [Fact]
    public async Task SetPhotoAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().SetPhotoAsync(Guid.NewGuid(), "x", Ct));

        Assert.Equal("User not found.", error.Message);
    }

    // ---------- UpsertMineAsync ----------

    [Fact]
    public async Task UpsertMineAsync_NewProfile_CreatesItWithTrimmedFieldsAndPendingStatus()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        UserProfile? saved = null;
        // The first GetByUserIdAsync call (before the upsert) must see no profile; the second (the
        // service's own re-fetch, to build the returned DTO) must see what AddAsync just captured.
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(_ => saved);
        await _profiles.AddAsync(Arg.Do<UserProfile>(p => saved = p), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().UpsertMineAsync(user.Id, UpsertRequest(), Ct);

        Assert.NotNull(saved);
        Assert.Equal(user.Id, saved.UserId);
        Assert.Equal("Rahima Begum", saved.LegalName);
        Assert.Equal("+8801712345678", saved.Phone);
        Assert.Equal("1234567890", saved.NidNumber);
        Assert.Equal("House 12", saved.AddressLine);
        Assert.Equal(UserProfileStatus.Pending, saved.Status);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _profiles.Received(1).AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
        await _profiles.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Pending", dto.Status);
    }

    [Fact]
    public async Task UpsertMineAsync_ExistingProfile_UpdatesInPlaceAndResetsToPendingClearingTheOldReview()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), UserId = user.Id, LegalName = "Old Name", Status = UserProfileStatus.Rejected,
            ReviewedByUserId = Guid.NewGuid(), ReviewedAt = DateTime.UtcNow.AddDays(-1), ReviewNotes = "old note",
        };
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(profile);

        await CreateService().UpsertMineAsync(user.Id, UpsertRequest(), Ct);

        Assert.Equal("Rahima Begum", profile.LegalName);
        Assert.Equal(UserProfileStatus.Pending, profile.Status);
        Assert.Null(profile.ReviewedByUserId);
        Assert.Null(profile.ReviewedAt);
        Assert.Null(profile.ReviewNotes);
        await _profiles.DidNotReceive().AddAsync(Arg.Any<UserProfile>(), Arg.Any<CancellationToken>());
        await _profiles.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertMineAsync_EmptyOptionalFields_AreStoredAsNull()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        UserProfile? saved = null;
        await _profiles.AddAsync(Arg.Do<UserProfile>(p => saved = p), Arg.Any<CancellationToken>());
        var request = UpsertRequest();
        request.Expertise = "   ";
        request.About = "   ";

        await CreateService().UpsertMineAsync(user.Id, request, Ct);

        Assert.Null(saved!.Expertise);
        Assert.Null(saved.About);
    }

    [Fact]
    public async Task UpsertMineAsync_ProducerWithoutExpertise_ThrowsConflictAndSavesNothing()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().UpsertMineAsync(user.Id, UpsertRequest(), Ct));

        Assert.Equal("Producers must state their expertise (for example Jamdani weaving or pottery).", error.Message);
        await _profiles.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertMineAsync_ProducerWithExpertise_Succeeds()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        UserProfile? saved = null;
        _profiles.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(_ => saved);
        await _profiles.AddAsync(Arg.Do<UserProfile>(p => saved = p), Arg.Any<CancellationToken>());
        var request = UpsertRequest();
        request.Expertise = "Jamdani weaving";

        var dto = await CreateService().UpsertMineAsync(user.Id, request, Ct);

        Assert.Equal("Jamdani weaving", dto.Expertise);
    }

    [Fact]
    public async Task UpsertMineAsync_DistrictDoesNotExist_ThrowsConflict()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.DistrictExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var request = UpsertRequest();
        request.DistrictId = Guid.NewGuid();

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().UpsertMineAsync(user.Id, request, Ct));

        Assert.Equal("District not found.", error.Message);
    }

    [Fact]
    public async Task UpsertMineAsync_NidAlreadyUsedByAnotherAccount_ThrowsConflict()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Customer);
        _users.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _profiles.NidInUseAsync("1234567890", user.Id, Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().UpsertMineAsync(user.Id, UpsertRequest(), Ct));

        Assert.Equal("This NID number is already registered to another account.", error.Message);
    }

    [Fact]
    public async Task UpsertMineAsync_UnknownUser_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().UpsertMineAsync(Guid.NewGuid(), UpsertRequest(), Ct));

        Assert.Equal("User not found.", error.Message);
    }

    // ---------- GetForAdminAsync ----------

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 51, 2, 20)]
    [InlineData(3, 50, 3, 50)]
    public async Task GetForAdminAsync_KeepsPageAndSizeWithinBounds(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _profiles.GetPagedAsync(Arg.Any<UserProfileQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<UserProfile>(), 0));
        _profiles.GetRolesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, List<string>>());

        var result = await CreateService().GetForAdminAsync(new UserProfileQueryParameters { Page = page, PageSize = pageSize }, Ct);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedSize, result.PageSize);
    }

    [Fact]
    public async Task GetForAdminAsync_MapsItemsWithTheirRoles()
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "x", Phone = "x", NidNumber = "x", AddressLine = "x",
            User = TestUsers.Create(fullName: "Rahima Begum"),
        };
        _profiles.GetPagedAsync(Arg.Any<UserProfileQueryParameters>(), Arg.Any<CancellationToken>())
            .Returns((new List<UserProfile> { profile }, 5));
        _profiles.GetRolesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, List<string>> { [profile.UserId] = new() { RoleNames.Producer } });

        var result = await CreateService().GetForAdminAsync(new UserProfileQueryParameters(), Ct);

        Assert.Equal(5, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal("Rahima Begum", dto.LoginName);
        Assert.Equal(new[] { RoleNames.Producer }, dto.Roles);
    }

    [Fact]
    public async Task GetForAdminAsync_UserWithNoRolesFound_GetsAnEmptyRolesList()
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "x", Phone = "x", NidNumber = "x", AddressLine = "x",
            User = TestUsers.Create(),
        };
        _profiles.GetPagedAsync(Arg.Any<UserProfileQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<UserProfile> { profile }, 1));
        _profiles.GetRolesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, List<string>>());

        var result = await CreateService().GetForAdminAsync(new UserProfileQueryParameters(), Ct);

        Assert.Empty(Assert.Single(result.Items).Roles);
    }

    // ---------- ApproveAsync / RejectAsync ----------

    private UserProfile PendingProfile(UserProfileStatus status = UserProfileStatus.Pending)
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "x", Phone = "x", NidNumber = "x", AddressLine = "x",
            Status = status, User = TestUsers.Create(),
        };
        _profiles.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        _profiles.GetRolesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, List<string>>());
        return profile;
    }

    [Fact]
    public async Task ApproveAsync_PendingProfile_ApprovesItAndRecordsTheReviewer()
    {
        var profile = PendingProfile();
        var admin = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var dto = await CreateService().ApproveAsync(profile.Id, admin, new ReviewUserProfileRequest(), Ct);

        Assert.Equal(UserProfileStatus.Approved, profile.Status);
        Assert.Equal(admin, profile.ReviewedByUserId);
        Assert.InRange(profile.ReviewedAt!.Value, before, DateTime.UtcNow);
        Assert.Equal("Approved", dto.Status);
        await _profiles.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_NotesGiven_AreTrimmedAndStored()
    {
        var profile = PendingProfile();

        await CreateService().ApproveAsync(profile.Id, Guid.NewGuid(), new ReviewUserProfileRequest { Notes = "  Looks good.  " }, Ct);

        Assert.Equal("Looks good.", profile.ReviewNotes);
    }

    [Theory]
    [InlineData(UserProfileStatus.Approved)]
    [InlineData(UserProfileStatus.Rejected)]
    public async Task ApproveAsync_AlreadyReviewedProfile_ThrowsConflictAndSavesNothing(UserProfileStatus status)
    {
        var profile = PendingProfile(status);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().ApproveAsync(profile.Id, Guid.NewGuid(), new ReviewUserProfileRequest(), Ct));

        Assert.Equal("Only a profile that is waiting for review can be approved or rejected.", error.Message);
        await _profiles.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_UnknownProfile_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().ApproveAsync(Guid.NewGuid(), Guid.NewGuid(), new ReviewUserProfileRequest(), Ct));

        Assert.Equal("Profile not found.", error.Message);
    }

    [Fact]
    public async Task RejectAsync_WithAReason_RejectsTheProfile()
    {
        var profile = PendingProfile();

        var dto = await CreateService().RejectAsync(profile.Id, Guid.NewGuid(), new ReviewUserProfileRequest { Notes = "Blurry NID photo." }, Ct);

        Assert.Equal(UserProfileStatus.Rejected, profile.Status);
        Assert.Equal("Rejected", dto.Status);
    }

    [Fact]
    public async Task RejectAsync_NoReasonGiven_ThrowsConflictAndSavesNothing()
    {
        var profile = PendingProfile();

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().RejectAsync(profile.Id, Guid.NewGuid(), new ReviewUserProfileRequest(), Ct));

        Assert.Equal("Tell the member why the profile was rejected so they can fix it.", error.Message);
        await _profiles.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_ReasonIsOnlyWhitespace_ThrowsConflict()
    {
        var profile = PendingProfile();

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().RejectAsync(profile.Id, Guid.NewGuid(), new ReviewUserProfileRequest { Notes = "   " }, Ct));
    }
}
