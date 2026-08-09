namespace ShilpoHubBD.Domain.Entities.ProductDevelopment;

public class PrototypeFile
{
    public Guid Id { get; set; }

    public Guid PrototypeVersionId { get; set; }
    public PrototypeVersion PrototypeVersion { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
