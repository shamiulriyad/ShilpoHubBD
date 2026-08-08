namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class FamilyHeritageMemberInput
{
    public string FullName { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public int Generation { get; set; }
    public string? Role { get; set; }
    public string? ActiveYearsRange { get; set; }
    public string? Story { get; set; }
}
