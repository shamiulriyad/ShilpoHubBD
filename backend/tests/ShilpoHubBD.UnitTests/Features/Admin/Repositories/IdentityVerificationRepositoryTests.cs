using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Admin;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Admin.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Admin")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class IdentityVerificationRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public IdentityVerificationRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>UserId is filled in by <see cref="SeedUserAsync"/> once the owning user exists.</summary>
    private static IdentityVerificationRequest Request(
        IdentityVerificationStatus status, IdentityVerificationType type = IdentityVerificationType.NationalId,
        DateTime? submittedAt = null, string documentNumber = "DOC-1") => new()
    {
        Id = Guid.NewGuid(), Type = type, Status = status, DocumentNumber = documentNumber,
        FrontImageUrl = "front.jpg", SubmittedAt = submittedAt ?? DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    private static async Task<User> SeedUserAsync(ShilpoHubDbContext context, string fullName, string email, params IdentityVerificationRequest[] requests)
    {
        var user = TestUsers.Create(email, fullName);
        context.Users.Add(user);
        foreach (var r in requests)
        {
            r.UserId = user.Id;
            context.IdentityVerificationRequests.Add(r);
        }

        await context.SaveChangesAsync(Ct);
        return user;
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheRequest()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Karim Sheikh", "karim3@example.com");
        var request = Request(IdentityVerificationStatus.Pending);
        request.UserId = user.Id;
        var repository = new IdentityVerificationRepository(db.NewContext());

        await repository.AddAsync(request, Ct);
        await repository.SaveChangesAsync(Ct);

        Assert.True(await db.NewContext().IdentityVerificationRequests.AnyAsync(r => r.Id == request.Id, Ct));
    }

    [Fact]
    public async Task GetByIdAsync_LoadsTheApplicantAndReviewer()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var reviewer = TestUsers.Create(fullName: "Admin Person");
        context.Users.Add(reviewer);
        var user = await SeedUserAsync(context, "Rahima Begum", "rahima3@example.com");
        var request = Request(IdentityVerificationStatus.Approved);
        request.UserId = user.Id;
        request.ReviewedByUserId = reviewer.Id;
        context.IdentityVerificationRequests.Add(request);
        await context.SaveChangesAsync(Ct);

        var found = await new IdentityVerificationRepository(db.NewContext()).GetByIdAsync(request.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal("Rahima Begum", found.User.FullName);
        Assert.Equal("Admin Person", found.ReviewedBy!.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = await _database.BeginAsync();

        Assert.Null(await new IdentityVerificationRepository(db.NewContext()).GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetPagedAsync_ListsPendingBeforeReviewedRequestsNewestFirstWithinEachGroup()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var context = db.NewContext();
        var user = await SeedUserAsync(context, "Timeline User", $"timeline-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Approved, submittedAt: now.AddDays(-1), documentNumber: "REVIEWED"),
            Request(IdentityVerificationStatus.Pending, submittedAt: now.AddDays(-3), documentNumber: "OLD-PENDING"),
            Request(IdentityVerificationStatus.Pending, submittedAt: now, documentNumber: "NEW-PENDING"));

        var (items, total) = await new IdentityVerificationRepository(db.NewContext())
            .GetPagedAsync(new IdentityVerificationQueryParameters { UserId = user.Id, PageSize = 10 }, Ct);

        Assert.Equal(3, total);
        Assert.Equal(new[] { "NEW-PENDING", "OLD-PENDING", "REVIEWED" }, items.Select(r => r.DocumentNumber));
    }

    [Fact]
    public async Task GetPagedAsync_ByStatus_ReturnsOnlyThatStatus()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Status User", $"status-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending), Request(IdentityVerificationStatus.Rejected));

        var (items, total) = await new IdentityVerificationRepository(db.NewContext())
            .GetPagedAsync(new IdentityVerificationQueryParameters { UserId = user.Id, Status = "Rejected" }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(IdentityVerificationStatus.Rejected, Assert.Single(items).Status);
    }

    [Fact]
    public async Task GetPagedAsync_ByType_ReturnsOnlyThatType()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Type User", $"type-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending, IdentityVerificationType.Passport),
            Request(IdentityVerificationStatus.Pending, IdentityVerificationType.NationalId));

        var (items, total) = await new IdentityVerificationRepository(db.NewContext())
            .GetPagedAsync(new IdentityVerificationQueryParameters { UserId = user.Id, Type = "Passport" }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(IdentityVerificationType.Passport, Assert.Single(items).Type);
    }

    [Fact]
    public async Task GetPagedAsync_UnknownStatusOrType_IsIgnoredAndReturnsEverything()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Ignore User", $"ignore-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending));

        var (items, total) = await new IdentityVerificationRepository(db.NewContext())
            .GetPagedAsync(new IdentityVerificationQueryParameters { UserId = user.Id, Status = "Bogus" }, Ct);

        Assert.Equal(1, total);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetPagedAsync_Search_MatchesApplicantNameEmailOrDocumentNumber()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Findable Applicant", $"findable-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending, documentNumber: "UNIQUE-DOC-123"));

        var (items, total) = await new IdentityVerificationRepository(db.NewContext())
            .GetPagedAsync(new IdentityVerificationQueryParameters { Search = "unique-doc-123" }, Ct);

        Assert.Equal(1, total);
        Assert.Equal(user.Id, Assert.Single(items).UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsThatUsersRequestsNewestFirst()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var user = await SeedUserAsync(db.NewContext(), "History User", $"history-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Rejected, submittedAt: now.AddDays(-2), documentNumber: "OLD"),
            Request(IdentityVerificationStatus.Pending, submittedAt: now, documentNumber: "NEW"));
        var other = await SeedUserAsync(db.NewContext(), "Other User", $"other-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending, documentNumber: "OTHER"));

        var result = await new IdentityVerificationRepository(db.NewContext()).GetByUserIdAsync(user.Id, Ct);

        Assert.Equal(new[] { "NEW", "OLD" }, result.Select(r => r.DocumentNumber));
    }

    [Fact]
    public async Task GetLatestStatusesByUserIdsAsync_ReturnsTheMostRecentStatusPerUser()
    {
        await using var db = await _database.BeginAsync();
        var now = DateTime.UtcNow;
        var userA = await SeedUserAsync(db.NewContext(), "User A", $"a-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Rejected, submittedAt: now.AddDays(-1)),
            Request(IdentityVerificationStatus.Pending, submittedAt: now));
        var userB = await SeedUserAsync(db.NewContext(), "User B", $"b-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Approved, submittedAt: now));
        var userC = await SeedUserAsync(db.NewContext(), "User C", $"c-{Guid.NewGuid():N}@example.com");

        var statuses = await new IdentityVerificationRepository(db.NewContext())
            .GetLatestStatusesByUserIdsAsync(new[] { userA.Id, userB.Id, userC.Id }, Ct);

        Assert.Equal(IdentityVerificationStatus.Pending, statuses[userA.Id]);
        Assert.Equal(IdentityVerificationStatus.Approved, statuses[userB.Id]);
        Assert.False(statuses.ContainsKey(userC.Id));
    }

    [Fact]
    public async Task HasPendingRequestAsync_TrueOnlyWhileAPendingRequestExists()
    {
        await using var db = await _database.BeginAsync();
        var withPending = await SeedUserAsync(db.NewContext(), "Pending User", $"pending-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Pending));
        var withoutPending = await SeedUserAsync(db.NewContext(), "Approved User", $"approved-{Guid.NewGuid():N}@example.com",
            Request(IdentityVerificationStatus.Approved));
        var repository = new IdentityVerificationRepository(db.NewContext());

        Assert.True(await repository.HasPendingRequestAsync(withPending.Id, Ct));
        Assert.False(await repository.HasPendingRequestAsync(withoutPending.Id, Ct));
    }

    [Fact]
    public async Task UserExistsAsync_ReportsWhetherTheUserRowExists()
    {
        await using var db = await _database.BeginAsync();
        var user = await SeedUserAsync(db.NewContext(), "Exists User", $"exists-{Guid.NewGuid():N}@example.com");
        var repository = new IdentityVerificationRepository(db.NewContext());

        Assert.True(await repository.UserExistsAsync(user.Id, Ct));
        Assert.False(await repository.UserExistsAsync(Guid.NewGuid(), Ct));
    }
}
