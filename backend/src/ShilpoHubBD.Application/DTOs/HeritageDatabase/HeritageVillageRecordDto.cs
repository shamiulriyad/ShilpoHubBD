namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageVillageRecordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Craft { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
