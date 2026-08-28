using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

/// <summary>
/// An export request against a dataset. The backend records export metadata and analytics only;
/// it does not generate files. <see cref="FileUrl"/> is a reference populated by an external worker.
/// </summary>
public class HeritageDatasetExport
{
    public Guid Id { get; set; }

    public Guid HeritageDatasetId { get; set; }
    public HeritageDataset Dataset { get; set; } = null!;

    public Guid? DatasetVersionId { get; set; }
    public HeritageDatasetVersion? Version { get; set; }

    public Guid RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;

    public HeritageDatasetFileFormat Format { get; set; } = HeritageDatasetFileFormat.Csv;
    public int RowCount { get; set; }
    public string? FilterJson { get; set; }
    public string? Notes { get; set; }

    public HeritageDatasetExportStatus Status { get; set; } = HeritageDatasetExportStatus.Pending;
    public string? FileUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
