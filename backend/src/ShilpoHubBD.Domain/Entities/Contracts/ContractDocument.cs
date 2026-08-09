namespace ShilpoHubBD.Domain.Entities.Contracts;

public class ContractDocument
{
    public Guid Id { get; set; }

    public Guid ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
