namespace ShilpoHubBD.Application.DTOs.AITourism;

public class TransportOptionDto
{
    public string Mode { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceClasses { get; set; }
    public string? Schedule { get; set; }
    public string? DurationText { get; set; }
    public decimal? FareBdt { get; set; }
    public string? FareNote { get; set; }
    public string? BookingUrl { get; set; }
    public string? Notes { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "SecondarySource";
    public string? UnverifiedFields { get; set; }
    public DateTime DataRetrievedOn { get; set; }
}
