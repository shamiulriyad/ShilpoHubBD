using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.MentorMatching.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "MentorMatching")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class MentorMatchingRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public MentorMatchingRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<MentorProfile> SeedMentorAsync(ShilpoHubDbContext context, bool isActive = true,
        string? location = null, string? availabilityNote = null, string? preferredCategory = null, int yearsOfExperience = 5)
    {
        var user = TestUsers.Create();
        var mentor = new MentorProfile
        {
            Id = Guid.NewGuid(), UserId = user.Id, User = user, Bio = "Loves teaching weaving", Expertise = "Weaving",
            YearsOfExperience = yearsOfExperience, IsActive = isActive, Location = location, AvailabilityNote = availabilityNote,
            PreferredCategory = preferredCategory, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        context.Users.Add(user);
        context.MentorProfiles.Add(mentor);
        await context.SaveChangesAsync(Ct);
        return mentor;
    }

    [Fact]
    public async Task GetCandidatesAsync_InactiveMentor_IsExcluded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext, isActive: false);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest(), Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCandidatesAsync_ActiveMentor_ReturnsBasicFieldsFromTheProfile()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest(), Ct);

        var candidate = Assert.Single(result);
        Assert.Equal(mentor.Id, candidate.MentorProfileId);
        Assert.Equal(mentor.User.FullName, candidate.FullName);
        Assert.Equal(mentor.YearsOfExperience, candidate.YearsOfExperience);
    }

    [Fact]
    public async Task GetCandidatesAsync_NoHeritageSkillIdInRequest_LeavesSkillFieldsUnset()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest(), Ct);

        var candidate = Assert.Single(result);
        Assert.False(candidate.HasMatchingSkill);
        Assert.Null(candidate.MatchingSkillLevel);
    }

    [Fact]
    public async Task GetCandidatesAsync_MentorHasTheRequestedSkill_SetsHasMatchingSkillAndLevel()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var mentor = await SeedMentorAsync(seedContext);
        var skill = new HeritageSkill { Id = Guid.NewGuid(), Name = "Weaving " + Guid.NewGuid().ToString("N")[..8], Description = "x", CreatedAt = DateTime.UtcNow };
        seedContext.HeritageSkills.Add(skill);
        seedContext.MentorSkills.Add(new MentorSkill { Id = Guid.NewGuid(), MentorProfileId = mentor.Id, HeritageSkillId = skill.Id, Level = SkillLevel.Expert, AddedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { HeritageSkillId = skill.Id }, Ct);

        var candidate = Assert.Single(result);
        Assert.True(candidate.HasMatchingSkill);
        Assert.Equal(SkillLevel.Expert, candidate.MatchingSkillLevel);
    }

    [Fact]
    public async Task GetCandidatesAsync_MentorDoesNotHaveTheRequestedSkill_LeavesItUnmatched()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid() }, Ct);

        Assert.False(Assert.Single(result).HasMatchingSkill);
    }

    [Fact]
    public async Task GetCandidatesAsync_LocationContainsTheRequestedTextCaseInsensitively_SetsHasMatchingLocation()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext, location: "Old Dhaka");

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { Location = "dhaka" }, Ct);

        Assert.True(Assert.Single(result).HasMatchingLocation);
    }

    [Fact]
    public async Task GetCandidatesAsync_GoalKeywordMatchesBioOrExpertise_SetsHasMatchingGoalKeyword()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext);

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { LearningGoalKeyword = "weaving" }, Ct);

        Assert.True(Assert.Single(result).HasMatchingGoalKeyword);
    }

    [Fact]
    public async Task GetCandidatesAsync_AvailabilityKeywordMatches_SetsHasMatchingAvailability()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext, availabilityNote: "Available on weekends");

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { AvailabilityKeyword = "weekends" }, Ct);

        Assert.True(Assert.Single(result).HasMatchingAvailability);
    }

    [Fact]
    public async Task GetCandidatesAsync_PreferredCategoryMatches_SetsHasMatchingCategory()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        await SeedMentorAsync(seedContext, preferredCategory: "Textiles");

        var repository = new MentorMatchingRepository(scope.NewContext());
        var result = await repository.GetCandidatesAsync(new MentorMatchRequest { PreferredCategory = "textiles" }, Ct);

        Assert.True(Assert.Single(result).HasMatchingCategory);
    }
}
