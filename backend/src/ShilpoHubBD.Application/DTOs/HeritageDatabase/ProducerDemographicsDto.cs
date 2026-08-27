using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class ProducerDemographicsDto
{
    public int TotalProducers { get; set; }
    public int WithHeritageIdentity { get; set; }
    public int VerifiedHeritageIdentity { get; set; }
    public double AverageYearsOfExperience { get; set; }
    public List<HeritageCountBucketDto> ByDivision { get; set; } = new();
    public List<HeritageCountBucketDto> ByDistrict { get; set; } = new();
    public List<HeritageCountBucketDto> ByPrimaryCraft { get; set; } = new();
    public List<HeritageCountBucketDto> ByExperienceBand { get; set; } = new();
    public List<HeritageCountBucketDto> ByVerificationStatus { get; set; } = new();
}
