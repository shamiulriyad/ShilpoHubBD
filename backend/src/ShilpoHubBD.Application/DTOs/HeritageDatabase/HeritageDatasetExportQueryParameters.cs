namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetExportQueryParameters
{
    public string? Status { get; set; }
    public string? Format { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
