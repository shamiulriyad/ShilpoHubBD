namespace ShilpoHubBD.Application.DTOs.Cms;

public class CmsEventQueryParameters
{
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
