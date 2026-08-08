namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

// One row per Heritage Legacy Score (re)calculation, for the "score history" requirement.
// The breakdown is snapshotted alongside the total so past scores stay explainable even if
// the configurable weights change later.
public class ScoreHistoryEntry
{
    public Guid Id { get; set; }

    public Guid ProducerHeritageIdentityId { get; set; }
    public ProducerHeritageIdentity ProducerHeritageIdentity { get; set; } = null!;

    public int Score { get; set; }

    public int YearsOfExperiencePoints { get; set; }
    public int VerificationPoints { get; set; }
    public int AwardsPoints { get; set; }
    public int CertificationsPoints { get; set; }
    public int ProductsPoints { get; set; }
    public int ReviewsPoints { get; set; }
    public int ApprenticesTrainedPoints { get; set; }
    public int CoursesPublishedPoints { get; set; }
    public int CulturalContributionPoints { get; set; }

    public DateTime CalculatedAt { get; set; }
}
