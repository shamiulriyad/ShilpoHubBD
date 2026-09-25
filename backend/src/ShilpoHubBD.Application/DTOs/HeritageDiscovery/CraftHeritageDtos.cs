namespace ShilpoHubBD.Application.DTOs.HeritageDiscovery;

public class CraftHeritageEntryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Aliases { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? GiName { get; set; }
    public string? Unesco { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? History { get; set; }
    public string? Materials { get; set; }
    public string? Process { get; set; }
    public string? Products { get; set; }
    public string? Story { get; set; }
    public string? Visit { get; set; }
    public string? Sources { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Used for both create and update (IsActive is ignored on create).</summary>
public class SaveCraftHeritageEntryRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Aliases { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? GiName { get; set; }
    public string? Unesco { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? History { get; set; }
    public string? Materials { get; set; }
    public string? Process { get; set; }
    public string? Products { get; set; }
    public string? Story { get; set; }
    public string? Visit { get; set; }
    public string? Sources { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
