namespace ShilpoHubBD.Application.DTOs.Learning;

public class UpdateMentorProfileRequest
{
    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public bool IsActive { get; set; } = true;
}
