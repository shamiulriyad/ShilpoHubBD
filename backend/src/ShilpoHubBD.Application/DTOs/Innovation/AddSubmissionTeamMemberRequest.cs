namespace ShilpoHubBD.Application.DTOs.Innovation;

public class AddSubmissionTeamMemberRequest
{
    public Guid UserId { get; set; }
    public string? RoleOnTeam { get; set; }
}
