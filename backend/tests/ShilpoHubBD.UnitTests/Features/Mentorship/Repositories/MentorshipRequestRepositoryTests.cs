using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.Mentorship;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Mentorship.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Mentorship")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class MentorshipRequestRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public MentorshipRequestRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<MentorProfile> SeedMentorAsync(ShilpoHubDbContext context)
    {
        var user = TestUsers.Create();
        var mentor = new MentorProfile
        {
            Id = Guid.NewGuid(), UserId = user.Id, User = user, Bio = "x", Expertise = "Weaving", YearsOfExperience = 5,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Users.Add(user);
        context.MentorProfiles.Add(mentor);
        await context.SaveChangesAsync(Ct);
        return mentor;
    }

    private static MentorshipRequest MakeRequest(MentorProfile mentor, Guid learnerUserId, MentorshipRequestStatus status = MentorshipRequestStatus.Pending) => new()
    {
        Id = Guid.NewGuid(), MentorProfileId = mentor.Id, LearnerUserId = learnerUserId,
        Message = "Please teach me", Status = status, RequestedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new MentorshipRequestRepository(scope.NewContext());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItWithMentorAndLearnerLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var learner = TestUsers.Create();
        seedContext.Users.Add(learner);
        var mentorshipRequest = MakeRequest(mentor, learner.Id);
        seedContext.MentorshipRequests.Add(mentorshipRequest);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorshipRequestRepository(scope.NewContext());
        var found = await repository.GetByIdAsync(mentorshipRequest.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(mentor.User.FullName, found!.MentorProfile.User.FullName);
        Assert.Equal(learner.FullName, found.Learner.FullName);
    }

    [Fact]
    public async Task GetByLearnerAsync_ReturnsOnlyThatLearnersRequests_NewestFirst()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var learner = TestUsers.Create();
        var otherLearner = TestUsers.Create();
        seedContext.Users.AddRange(learner, otherLearner);
        var older = MakeRequest(mentor, learner.Id);
        older.RequestedAt = DateTime.UtcNow.AddDays(-1);
        var newer = MakeRequest(mentor, learner.Id);
        newer.RequestedAt = DateTime.UtcNow;
        var somebodyElses = MakeRequest(mentor, otherLearner.Id);
        seedContext.MentorshipRequests.AddRange(older, newer, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorshipRequestRepository(scope.NewContext());
        var result = await repository.GetByLearnerAsync(learner.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(r => r.Id));
    }

    [Fact]
    public async Task GetByMentorProfileAsync_ReturnsOnlyThatMentorsRequests()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var otherMentor = await SeedMentorAsync(seedContext);
        var learner = TestUsers.Create();
        seedContext.Users.Add(learner);
        var mine = MakeRequest(mentor, learner.Id);
        var somebodyElses = MakeRequest(otherMentor, learner.Id);
        seedContext.MentorshipRequests.AddRange(mine, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorshipRequestRepository(scope.NewContext());
        var result = await repository.GetByMentorProfileAsync(mentor.Id, Ct);

        Assert.Equal(mine.Id, Assert.Single(result).Id);
    }

    [Theory]
    [InlineData(MentorshipRequestStatus.Pending, true)]
    [InlineData(MentorshipRequestStatus.Accepted, true)]
    [InlineData(MentorshipRequestStatus.Rejected, false)]
    [InlineData(MentorshipRequestStatus.Completed, false)]
    public async Task HasOpenRequestAsync_ReflectsWhetherAPendingOrAcceptedRequestExists(MentorshipRequestStatus status, bool expected)
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var learner = TestUsers.Create();
        seedContext.Users.Add(learner);
        seedContext.MentorshipRequests.Add(MakeRequest(mentor, learner.Id, status));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorshipRequestRepository(scope.NewContext());

        Assert.Equal(expected, await repository.HasOpenRequestAsync(mentor.Id, learner.Id, Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheRequest()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var learner = TestUsers.Create();
        seedContext.Users.Add(learner);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorshipRequestRepository(scope.NewContext());
        var mentorshipRequest = MakeRequest(mentor, learner.Id);
        await repository.AddAsync(mentorshipRequest, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new MentorshipRequestRepository(scope.NewContext()).GetByIdAsync(mentorshipRequest.Id, Ct);
        Assert.NotNull(reloaded);
        Assert.Equal("Please teach me", reloaded!.Message);
    }
}
