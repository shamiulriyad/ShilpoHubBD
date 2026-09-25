namespace ShilpoHubBD.Application.DTOs.Certificates;

public class ExpertiseCertificateDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public DateTime IssuedAt { get; set; }
    public bool IsRevoked { get; set; }
}

public class ExpertiseLevelRuleDto
{
    public string Level { get; set; } = string.Empty;
    public int MinRatings { get; set; }
    public decimal MinAverage { get; set; }
}

// A producer's own standing: their ratings, the level those earn, and the certificates issued so far.
public class ExpertiseProgressDto
{
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public string? EarnedLevel { get; set; }
    public string? HighestIssuedLevel { get; set; }
    public string? NextLevel { get; set; }
    public bool AwaitingAdmin { get; set; }
    public bool ProfileApproved { get; set; }
    public List<ExpertiseLevelRuleDto> Rules { get; set; } = new();
    public List<ExpertiseCertificateDto> Certificates { get; set; } = new();
}

public class EligibleProducerDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public string EarnedLevel { get; set; } = string.Empty;
    public string? HighestIssuedLevel { get; set; }
    public bool ProfileApproved { get; set; }
}

public class IssueExpertiseCertificateRequest
{
    public Guid ProducerId { get; set; }
}
