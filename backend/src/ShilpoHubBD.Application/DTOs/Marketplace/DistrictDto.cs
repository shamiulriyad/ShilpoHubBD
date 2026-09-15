namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class DistrictDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateDistrictRequest
{
    public string Division { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
