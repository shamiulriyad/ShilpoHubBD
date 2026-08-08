namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class HeritageCertificationInput
{
    public string Name { get; set; } = string.Empty;
    public string IssuingBody { get; set; } = string.Empty;
    public int IssuedYear { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CertificateNumber { get; set; }
    public string? CertificateUrl { get; set; }
}
