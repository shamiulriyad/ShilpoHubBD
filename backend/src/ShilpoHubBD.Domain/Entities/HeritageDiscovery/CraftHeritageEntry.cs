namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

/// <summary>Editorial reference article about a traditional craft (GI / UNESCO recognition, history, process, sources).</summary>
public class CraftHeritageEntry
{
    public Guid Id { get; set; }

    /// <summary>URL-safe key; matches a craft category slug so the article shows on that craft's page.</summary>
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Comma-separated alternative names.</summary>
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

    /// <summary>One source per line as "Label | https://url".</summary>
    public string? Sources { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
