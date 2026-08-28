using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

/// <summary>An immutable snapshot description of a dataset at a point in time.</summary>
public class HeritageDatasetVersion
{
    public Guid Id { get; set; }

    public Guid HeritageDatasetId { get; set; }
    public HeritageDataset Dataset { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string? Label { get; set; }
    public string Changelog { get; set; } = string.Empty;

    public int RecordCount { get; set; }
    public HeritageDatasetFileFormat Format { get; set; } = HeritageDatasetFileFormat.None;

    // CSV/JSON import metadata (references only; no file is parsed or stored by the backend).
    public string? SourceFileName { get; set; }
    public string? SourceFileUrl { get; set; }
    public string? SourceContentHash { get; set; }
    public int? ImportedRowCount { get; set; }
    public int ImportErrorCount { get; set; }
    public string? ImportNotes { get; set; }

    /// <summary>Optional JSON describing the column schema of this version.</summary>
    public string? SchemaJson { get; set; }

    public bool IsCurrent { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
