using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetExportAnalyticsDto
{
    public Guid HeritageDatasetId { get; set; }
    public int TotalExports { get; set; }
    public int CompletedExports { get; set; }
    public long TotalRowsExported { get; set; }
    public DateTime? LastExportedAt { get; set; }
    public List<HeritageCountBucketDto> ByFormat { get; set; } = new();
    public List<HeritageCountBucketDto> ByMonth { get; set; } = new();
    public List<HeritageCountBucketDto> TopExporters { get; set; } = new();
}
