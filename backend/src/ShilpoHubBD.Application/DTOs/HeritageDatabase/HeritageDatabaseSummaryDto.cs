using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatabaseSummaryDto
{
    public int Districts { get; set; }
    public int Villages { get; set; }
    public int HeritageLocations { get; set; }
    public int Producers { get; set; }
    public int Products { get; set; }
    public int TourismServices { get; set; }
    public int RiskRecords { get; set; }
    public int Datasets { get; set; }
    public int PublishedDatasets { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<HeritageCountBucketDto> RiskByLevel { get; set; } = new();
    public List<HeritageCountBucketDto> DatasetsByCategory { get; set; } = new();
}
