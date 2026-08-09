namespace ShilpoHubBD.Application.DTOs.Investment;

public class AddInvestmentDocumentRequest
{
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}
