namespace ShilpoHubBD.Application.DTOs.QRVerification;

public class QRVerificationHistoryItemDto
{
    public Guid Id { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public DateTime VerifiedAt { get; set; }
}
