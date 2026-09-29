namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportQueryParameters
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ProducerId { get; set; }

    /// <summary>Matches against the producer's full name or email.</summary>
    public string? Search { get; set; }

    /// <summary>
    /// Restricts results to reports shared with this user. The Government/NGO-facing controller always
    /// overwrites this to the caller's own id after model binding — never trust a client-supplied value
    /// for this field to enforce "can only access reports shared with them".
    /// </summary>
    public Guid? SharedWithUserId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
