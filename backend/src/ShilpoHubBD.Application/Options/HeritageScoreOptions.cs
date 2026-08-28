namespace ShilpoHubBD.Application.Options;

// Configurable, rule-based weights for the Heritage Legacy Score (no AI/ML involved).
// Bound from the "HeritageScore" configuration section so weights can be tuned without a code change.
public class HeritageScoreOptions
{
    public int VerifiedBonusPoints { get; set; } = 50;
    public int PointsPerFamilyMember { get; set; } = 5;
    public int PointsPerSkillTimelineEntry { get; set; } = 3;
    public int PointsPerAward { get; set; } = 10;
    public int PointsPerCertification { get; set; } = 8;
    public int PointsPerStoryArchiveEntry { get; set; } = 4;
    public int PointsPerYearOfExperience { get; set; } = 2;
    public int MaxYearsOfExperienceForScoring { get; set; } = 30;

    // Added for the full Heritage Legacy Score (products, reviews, apprentices trained, courses
    // published) on top of the identity-verification weights above.
    public int PointsPerProduct { get; set; } = 2;
    public int MaxProductsForScoring { get; set; } = 50;
    public int PointsPerReviewReceived { get; set; } = 1;
    public int MaxReviewsForScoring { get; set; } = 200;
    public int PointsPerApprenticeTrained { get; set; } = 6;
    public int PointsPerPublishedCourse { get; set; } = 8;
}
