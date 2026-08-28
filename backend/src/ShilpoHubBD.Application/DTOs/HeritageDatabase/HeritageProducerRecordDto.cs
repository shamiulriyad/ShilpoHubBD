namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageProducerRecordDto
{
    public Guid ProducerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? PrimaryCraft { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? WorkshopName { get; set; }
    public int? EstablishedYear { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Division { get; set; }
    public string? HeritageVerificationStatus { get; set; }
    public int? LegacyScore { get; set; }
    public int ProductCount { get; set; }
    public DateTime JoinedAt { get; set; }
}
