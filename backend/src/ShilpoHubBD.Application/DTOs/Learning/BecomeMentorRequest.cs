namespace ShilpoHubBD.Application.DTOs.Learning;

public class BecomeMentorRequest
{
    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
}
