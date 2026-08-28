namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

public class SupplierCertificationDto
{
    public string Source { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IssuingBody { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
