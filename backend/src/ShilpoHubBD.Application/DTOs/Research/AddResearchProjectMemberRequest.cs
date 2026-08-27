namespace ShilpoHubBD.Application.DTOs.Research;

public class AddResearchProjectMemberRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
