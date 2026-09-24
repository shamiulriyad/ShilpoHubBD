namespace ShilpoHubBD.Domain.Entities.Tourism;

// A real operator / service that runs between an origin city and a destination district (a bus
// company, a Bangladesh Railway train, an airline). Sourced records only -- see
// Seed/TransportData. Anything the sources do not state (timings, fares, off days) is null and
// listed in UnverifiedFields; this is a directory of who serves the route, not a live timetable.
public class TransportOption
{
    public Guid Id { get; set; }
    public string Mode { get; set; } = string.Empty;              // Bus | Train | Plane
    public string OriginName { get; set; } = string.Empty;        // matched against the planner's origin text
    public string DestinationDistrict { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceClasses { get; set; }
    public string? Schedule { get; set; }                         // only if a source states it
    public string? DurationText { get; set; }
    public decimal? FareBdt { get; set; }                         // lowest reported fare
    public string? FareNote { get; set; }
    public string? BookingUrl { get; set; }                       // an official booking page only
    public string? Notes { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "SecondarySource";
    public string? UnverifiedFields { get; set; }
    public DateTime DataRetrievedOn { get; set; }
    public bool IsActive { get; set; } = true;
}
