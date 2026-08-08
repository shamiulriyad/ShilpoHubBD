namespace ShilpoHubBD.Application.DTOs.QRVerification;

public class QRVerificationResultDto
{
    public bool IsValid { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProducerName { get; set; }
    public string? District { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
}
