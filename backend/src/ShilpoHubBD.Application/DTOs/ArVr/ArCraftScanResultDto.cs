namespace ShilpoHubBD.Application.DTOs.ArVr;

public class ArCraftScanResultDto
{
    public bool IsRecognized { get; set; }
    public DateTime ScannedAt { get; set; }
    public ArScannedProductDto? Product { get; set; }
    public ArCraftOriginDto? CraftStory { get; set; }
    public ArProducerHeritageDto? ProducerStory { get; set; }
    public string? TraceabilitySummary { get; set; }
    public bool IsCertified { get; set; }
    public string? CertificateNumber { get; set; }
}
