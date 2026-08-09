using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.DTOs.BusinessPartner;

public class BusinessDocumentInput
{
    public BusinessDocumentType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
