namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchProjectMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? InvitedByUserId { get; set; }
    public DateTime JoinedAt { get; set; }
}
