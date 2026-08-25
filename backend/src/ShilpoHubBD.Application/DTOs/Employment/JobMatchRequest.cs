namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobMatchRequest
{
    public string? Location { get; set; }
    public int? YearsOfExperience { get; set; }
    public int MaxResults { get; set; } = 10;
}
