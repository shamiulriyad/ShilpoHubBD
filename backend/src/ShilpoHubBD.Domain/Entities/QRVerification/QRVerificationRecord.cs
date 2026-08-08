using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.QRVerification;

public class QRVerificationRecord
{
    public Guid Id { get; set; }

    public string ScannedCode { get; set; } = string.Empty;

    public Guid? QRCodeId { get; set; }
    public QRCode? QRCode { get; set; }

    public Guid? VerifiedByUserId { get; set; }
    public User? VerifiedByUser { get; set; }

    public bool IsValid { get; set; }
    public DateTime VerifiedAt { get; set; }
}
