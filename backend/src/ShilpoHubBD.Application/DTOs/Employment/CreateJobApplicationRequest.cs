namespace ShilpoHubBD.Application.DTOs.Employment;

public class CreateJobApplicationRequest
{
    public Guid JobListingId { get; set; }
    public string CoverMessage { get; set; } = string.Empty;
}
