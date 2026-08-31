namespace ShilpoHubBD.Application.DTOs.MentorMatching;

public class MentorMatchResultDto
{
    public Guid MentorProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Location { get; set; }
    public string? AvailabilityNote { get; set; }
    public string? PreferredCategory { get; set; }

    public decimal MatchScore { get; set; }
    public List<string> MatchReasons { get; set; } = new();
}
