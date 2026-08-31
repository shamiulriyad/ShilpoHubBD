namespace ShilpoHubBD.Application.DTOs.Innovation;

public class SubmissionTeamMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? RoleOnTeam { get; set; }
    public Guid AddedByUserId { get; set; }
    public DateTime AddedAt { get; set; }
}
