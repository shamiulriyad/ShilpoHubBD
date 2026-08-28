namespace ShilpoHubBD.Application.DTOs.Contracts;

public class AddContractDocumentRequest
{
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}
