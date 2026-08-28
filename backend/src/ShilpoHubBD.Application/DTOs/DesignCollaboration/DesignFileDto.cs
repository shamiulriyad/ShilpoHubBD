namespace ShilpoHubBD.Application.DTOs.DesignCollaboration;

public class DesignFileDto
{
    public Guid Id { get; set; }
    public Guid? RevisionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public Guid UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
