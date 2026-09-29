using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;
using MentorMatchingService = ShilpoHubBD.Application.Services.MentorMatching.MentorMatchingService;

namespace ShilpoHubBD.UnitTests.Features.MentorMatching.Services;

[Trait("Feature", "MentorMatching")]
[Trait("Layer", "Service")]
public class MentorMatchingServiceTests
{
    private readonly IMentorMatchingRepository _repository = Substitute.For<IMentorMatchingRepository>();
    private readonly MentorMatchingService _service;

    public MentorMatchingServiceTests()
    {
        _service = new MentorMatchingService(_repository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static MentorMatchCandidateDto MakeCandidate(int yearsOfExperience = 0) => new()
    {
        MentorProfileId = Guid.NewGuid(), UserId = Guid.NewGuid(), FullName = "Mentor", YearsOfExperience = yearsOfExperience,
    };

    [Fact]
    public async Task MatchAsync_NoFiltersSpecified_ScoresPurelyOnBaselineExperience()
    {
        var candidate = MakeCandidate(yearsOfExperience: 10);
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest(), Ct);

        // Only the baseline experience weight (5) applies; 10+ years achieves it fully -> 100%.
        Assert.Equal(100m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_ZeroExperienceAndNoFilters_ScoresZero()
    {
        var candidate = MakeCandidate(yearsOfExperience: 0);
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest(), Ct);

        Assert.Equal(0m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_MatchingSkill_AddsToScoreAndReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingSkill = true;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid() }, Ct);

        // Baseline experience weight (5) still counts toward the denominator even though this
        // candidate has 0 years, diluting the achieved skill weight (25): 25 / (5 + 25) * 100 = 83.3.
        var dto = Assert.Single(result);
        Assert.Equal(83.3m, dto.MatchScore);
        Assert.Contains("Teaches the requested heritage skill.", dto.MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_NotMatchingSkill_ScoresZeroForThatCriterion()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingSkill = false;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid() }, Ct);

        Assert.Equal(0m, Assert.Single(result).MatchScore);
    }

    [Fact]
    public async Task MatchAsync_MinSkillLevelOnlyConsideredWhenSkillMatches()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingSkill = true;
        candidate.MatchingSkillLevel = SkillLevel.Expert;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid(), MinSkillLevel = SkillLevel.Advanced }, Ct);

        // Skill (25) + skill level (15) achieved = 40, but 0 years of experience means the baseline
        // weight (5) still isn't achieved: 40 / (5 + 25 + 15) * 100 = 88.9.
        var dto = Assert.Single(result);
        Assert.Equal(88.9m, dto.MatchScore);
        Assert.Contains(dto.MatchReasons, r => r.Contains("Skill level"));
    }

    [Fact]
    public async Task MatchAsync_SkillLevelBelowMinimum_DoesNotGetTheLevelBonus()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingSkill = true;
        candidate.MatchingSkillLevel = SkillLevel.Beginner;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid(), MinSkillLevel = SkillLevel.Advanced }, Ct);

        var dto = Assert.Single(result);
        Assert.True(dto.MatchScore < 100m);
        Assert.DoesNotContain(dto.MatchReasons, r => r.Contains("Skill level"));
    }

    [Fact]
    public async Task MatchAsync_MatchingLocation_AddsReasonWithTrimmedLocation()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingLocation = true;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { Location = "  Dhaka  " }, Ct);

        Assert.Contains("Located in \"Dhaka\".", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MinYearsOfExperiencePartiallyMet_GivesPartialCredit()
    {
        var candidate = MakeCandidate(yearsOfExperience: 5);
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { MinYearsOfExperience = 10 }, Ct);

        var dto = Assert.Single(result);
        Assert.True(dto.MatchScore > 0m && dto.MatchScore < 100m);
        Assert.DoesNotContain(dto.MatchReasons, r => r.Contains("Meets the requested minimum"));
    }

    [Fact]
    public async Task MatchAsync_MinYearsOfExperienceMet_AddsTheMeetsReason()
    {
        var candidate = MakeCandidate(yearsOfExperience: 10);
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { MinYearsOfExperience = 10 }, Ct);

        Assert.Contains(Assert.Single(result).MatchReasons, r => r.Contains("Meets the requested minimum of 10"));
    }

    [Fact]
    public async Task MatchAsync_MatchingAvailability_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingAvailability = true;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { AvailabilityKeyword = "weekends" }, Ct);

        Assert.Contains("Availability matches \"weekends\".", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MatchingCategory_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingCategory = true;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { PreferredCategory = "Textiles" }, Ct);

        Assert.Contains("Prefers teaching in the \"Textiles\" category.", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_MatchingGoalKeyword_AddsReason()
    {
        var candidate = MakeCandidate();
        candidate.HasMatchingGoalKeyword = true;
        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>()).Returns(new List<MentorMatchCandidateDto> { candidate });

        var result = await _service.MatchAsync(new MentorMatchRequest { LearningGoalKeyword = "weaving" }, Ct);

        Assert.Contains("Profile matches your learning goal \"weaving\".", Assert.Single(result).MatchReasons);
    }

    [Fact]
    public async Task MatchAsync_OrdersByScoreThenYearsOfExperience_AndRespectsMaxResults()
    {
        var lowScore = MakeCandidate(yearsOfExperience: 1);
        var highScoreLessExperienced = MakeCandidate(yearsOfExperience: 5);
        highScoreLessExperienced.HasMatchingSkill = true;
        var highScoreMoreExperienced = MakeCandidate(yearsOfExperience: 10);
        highScoreMoreExperienced.HasMatchingSkill = true;

        _repository.GetCandidatesAsync(Arg.Any<MentorMatchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new List<MentorMatchCandidateDto> { lowScore, highScoreLessExperienced, highScoreMoreExperienced });

        var result = await _service.MatchAsync(new MentorMatchRequest { HeritageSkillId = Guid.NewGuid(), MaxResults = 2 }, Ct);

        Assert.Equal(2, result.Count);
        Assert.Equal(highScoreMoreExperienced.MentorProfileId, result[0].MentorProfileId);
        Assert.Equal(highScoreLessExperienced.MentorProfileId, result[1].MentorProfileId);
    }
}
