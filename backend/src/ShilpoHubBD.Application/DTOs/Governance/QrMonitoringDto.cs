namespace ShilpoHubBD.Application.DTOs.Governance;

public class QrMonitoringOverviewDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int TotalCodes { get; set; }
    public int ActiveCodes { get; set; }
    public int TotalScans { get; set; }
    public int ValidScans { get; set; }
    public int InvalidScans { get; set; }
    public int UnresolvedScans { get; set; }
    public int UniqueScanners { get; set; }
    public double InvalidScanRatePercent { get; set; }

    /// <summary>Products whose invalid-scan ratio looks anomalous.</summary>
    public List<QrProductStatDto> AnomalousProducts { get; set; } = new();

    /// <summary>Codes with the most scans in the window.</summary>
    public List<QrCodeStatDto> MostScannedCodes { get; set; } = new();
}

public class QrProductStatDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public string? ProducerName { get; set; }
    public int TotalScans { get; set; }
    public int InvalidScans { get; set; }
    public double InvalidRatePercent { get; set; }
}

public class QrCodeStatDto
{
    public Guid? QrCodeId { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public bool IsActive { get; set; }
    public int TotalScans { get; set; }
    public int InvalidScans { get; set; }
}
