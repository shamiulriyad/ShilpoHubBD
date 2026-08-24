namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassAttendanceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}
