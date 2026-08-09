namespace ShilpoHubBD.Domain.Entities.BusinessPartner;

public class BusinessDocument
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerProfileId { get; set; }
    public BusinessPartnerProfile BusinessPartnerProfile { get; set; } = null!;

    public BusinessDocumentType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime UploadedAt { get; set; }
}
